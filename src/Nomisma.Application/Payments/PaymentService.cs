using System.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Nomisma.Application.Abstractions.Auth;
using Nomisma.Application.Abstractions.Integrations;
using Nomisma.Application.Abstractions.Persistence;
using Nomisma.Application.Common.Exceptions;
using Nomisma.Application.Common.Validation;
using Nomisma.Domain.Entities;
using Nomisma.Domain.Enums;
using AppValidationException = Nomisma.Application.Common.Exceptions.ValidationException;

namespace Nomisma.Application.Payments;

public sealed class PaymentService : IPaymentService
{
    private readonly INomismaDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<CreatePaymentRequestDto> _createValidator;

    public PaymentService(
        INomismaDbContext dbContext,
        ICurrentUserService currentUser,
        IPaymentGateway paymentGateway,
        ITransactionManager transactionManager,
        IValidator<CreatePaymentRequestDto> createValidator)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _paymentGateway = paymentGateway;
        _transactionManager = transactionManager;
        _createValidator = createValidator;
    }

    public async Task<IReadOnlyList<PaymentResponseDto>> ListAsync(CancellationToken cancellationToken)
    {
        var query = _dbContext.Payments
            .AsNoTracking()
            .Include(payment => payment.Installment)
            .ThenInclude(installment => installment!.Loan)
            .AsQueryable();

        if (_currentUser.IsCustomer)
        {
            var customerId = _currentUser.CustomerId ?? throw new ForbiddenException("Musteri baglantisi bulunamadi.");
            query = query.Where(payment => payment.Installment != null
                && payment.Installment.Loan != null
                && payment.Installment.Loan.CustomerId == customerId);
        }

        var payments = await query
            .OrderByDescending(payment => payment.PaidAtUtc)
            .ToListAsync(cancellationToken);

        return payments.Select(PaymentMapper.ToDto).ToList();
    }

    public async Task<PaymentResponseDto> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _dbContext.Payments
            .AsNoTracking()
            .Include(item => item.Installment)
            .ThenInclude(item => item!.Loan)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new NotFoundException("Odeme bulunamadi.");

        EnsureCanAccess(payment.Installment?.Loan?.CustomerId);
        return PaymentMapper.ToDto(payment);
    }

    public async Task<PaymentResponseDto> CreateAsync(CreatePaymentRequestDto request, CancellationToken cancellationToken)
    {
        await _createValidator.ValidateAndThrowApplicationExceptionAsync(request, cancellationToken);

        var installment = await _dbContext.Installments
            .AsNoTracking()
            .Include(item => item.Payment)
            .Include(item => item.Loan)
            .FirstOrDefaultAsync(item => item.Id == request.InstallmentId, cancellationToken)
            ?? throw new NotFoundException("Taksit bulunamadi.");

        EnsureCanAccess(installment.Loan?.CustomerId);
        EnsureInstallmentPayable(installment, request.Amount);
        await EnsureEarlierInstallmentsPaidAsync(installment, cancellationToken);

        var gatewayResult = await _paymentGateway.AuthorizeAsync(new PaymentGatewayRequestDto(
            request.InstallmentId,
            request.Amount,
            request.CardHolderName,
            request.CardNumber,
            request.Cvv,
            request.ExpiryMonth,
            request.ExpiryYear), cancellationToken);

        if (!gatewayResult.Succeeded)
        {
            throw new ConflictException(gatewayResult.FailureReason ?? "Odeme gateway tarafindan reddedildi.");
        }

        return await _transactionManager.ExecuteAsync(async () =>
        {
            var trackedInstallment = await _dbContext.Installments
                .Include(item => item.Payment)
                .Include(item => item.Loan)
                .ThenInclude(loan => loan!.Installments)
                .FirstOrDefaultAsync(item => item.Id == request.InstallmentId, cancellationToken)
                ?? throw new NotFoundException("Taksit bulunamadi.");

            EnsureInstallmentPayable(trackedInstallment, request.Amount);
            await EnsureEarlierInstallmentsPaidAsync(trackedInstallment, cancellationToken);

            var paidAt = DateTimeOffset.UtcNow;
            var payment = new Payment
            {
                InstallmentId = trackedInstallment.Id,
                Amount = request.Amount,
                PaidAtUtc = paidAt,
                Status = PaymentStatus.Completed,
                GatewayStatus = gatewayResult.Status,
                GatewayTransactionId = gatewayResult.TransactionId,
                FailureReason = gatewayResult.FailureReason
            };

            trackedInstallment.Payment = payment;
            trackedInstallment.Status = InstallmentStatus.Paid;
            trackedInstallment.PaidAtUtc = paidAt;
            _dbContext.Payments.Add(payment);

            var loan = trackedInstallment.Loan
                ?? throw new NotFoundException("Kredi bulunamadi.");

            if (loan.Installments.All(item => item.Id == trackedInstallment.Id || item.Status == InstallmentStatus.Paid))
            {
                loan.Status = LoanStatus.Closed;
                loan.ClosedAtUtc = paidAt;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return PaymentMapper.ToDto(payment, trackedInstallment, loan);
        }, cancellationToken, IsolationLevel.Serializable);
    }

    private void EnsureCanAccess(Guid? customerId)
    {
        if (_currentUser.IsAdmin)
        {
            return;
        }

        if (customerId.HasValue && _currentUser.IsCustomer && _currentUser.CustomerId == customerId)
        {
            return;
        }

        throw new ForbiddenException("Bu odeme kaydina erisim yetkiniz yok.");
    }

    private static void EnsureInstallmentPayable(Installment installment, decimal requestedAmount)
    {
        if (installment.Payment is not null || installment.Status == InstallmentStatus.Paid)
        {
            throw new ConflictException("Bu taksit zaten odenmis.");
        }

        if (installment.Loan?.Status == LoanStatus.Closed)
        {
            throw new ConflictException("Kapatilmis kredi icin odeme alinamaz.");
        }

        if (requestedAmount != installment.Amount)
        {
            throw new AppValidationException("Odeme tutari tam taksit tutariyla eslesmelidir.");
        }
    }

    private async Task EnsureEarlierInstallmentsPaidAsync(Installment installment, CancellationToken cancellationToken)
    {
        var customerId = installment.Loan?.CustomerId
            ?? throw new NotFoundException("Kredi bulunamadi.");

        var hasUnpaidEarlierInstallment = await _dbContext.Installments
            .AnyAsync(item =>
                item.Id != installment.Id
                && item.Loan != null
                && item.Loan.CustomerId == customerId
                && (item.DueDate < installment.DueDate
                    || (item.LoanId == installment.LoanId && item.InstallmentNumber < installment.InstallmentNumber))
                && item.Status != InstallmentStatus.Paid,
                cancellationToken);

        if (hasUnpaidEarlierInstallment)
        {
            throw new ConflictException("Daha erken vadeli taksitler odenmeden sonraki taksit odenemez.");
        }
    }

}
