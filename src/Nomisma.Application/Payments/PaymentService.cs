using Microsoft.EntityFrameworkCore;
using Nomisma.Application.Abstractions.Auth;
using Nomisma.Application.Abstractions.Integrations;
using Nomisma.Application.Abstractions.Persistence;
using Nomisma.Application.Common.Exceptions;
using Nomisma.Domain.Entities;
using Nomisma.Domain.Enums;

namespace Nomisma.Application.Payments;

public sealed class PaymentService
{
    private readonly INomismaDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ITransactionManager _transactionManager;

    public PaymentService(
        INomismaDbContext dbContext,
        ICurrentUserService currentUser,
        IPaymentGateway paymentGateway,
        ITransactionManager transactionManager)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _paymentGateway = paymentGateway;
        _transactionManager = transactionManager;
    }

    public async Task<IReadOnlyList<PaymentDto>> ListAsync(CancellationToken cancellationToken)
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

        return payments.Select(Map).ToList();
    }

    public async Task<PaymentDto> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _dbContext.Payments
            .AsNoTracking()
            .Include(item => item.Installment)
            .ThenInclude(item => item!.Loan)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new NotFoundException("Odeme bulunamadi.");

        EnsureCanAccess(payment.Installment?.Loan?.CustomerId);
        return Map(payment);
    }

    public async Task<PaymentDto> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        ValidateRequest(request);

        var installment = await _dbContext.Installments
            .AsNoTracking()
            .Include(item => item.Payment)
            .Include(item => item.Loan)
            .FirstOrDefaultAsync(item => item.Id == request.InstallmentId, cancellationToken)
            ?? throw new NotFoundException("Taksit bulunamadi.");

        EnsureCanAccess(installment.Loan?.CustomerId);
        EnsureInstallmentPayable(installment, request.Amount);

        var gatewayResult = await _paymentGateway.AuthorizeAsync(new PaymentGatewayRequest(
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
            return Map(payment, trackedInstallment, loan);
        }, cancellationToken);
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
            throw new ValidationException("Odeme tutari tam taksit tutariyla eslesmelidir.");
        }
    }

    private static void ValidateRequest(CreatePaymentRequest request)
    {
        if (request.Amount <= 0)
        {
            throw new ValidationException("Odeme tutari sifirdan buyuk olmalidir.");
        }

        if (string.IsNullOrWhiteSpace(request.CardHolderName)
            || string.IsNullOrWhiteSpace(request.CardNumber)
            || string.IsNullOrWhiteSpace(request.Cvv))
        {
            throw new ValidationException("Kart bilgileri zorunludur.");
        }

        if (request.ExpiryMonth is < 1 or > 12 || request.ExpiryYear < DateTime.UtcNow.Year)
        {
            throw new ValidationException("Kart son kullanma tarihi gecersiz.");
        }
    }

    public static PaymentDto Map(Payment payment)
    {
        return Map(
            payment,
            payment.Installment ?? throw new InvalidOperationException("Payment installment is not loaded."),
            payment.Installment.Loan ?? throw new InvalidOperationException("Payment loan is not loaded."));
    }

    private static PaymentDto Map(Payment payment, Installment installment, Loan loan)
    {
        return new PaymentDto(
            payment.Id,
            payment.InstallmentId,
            installment.LoanId,
            loan.CustomerId,
            payment.Amount,
            payment.PaidAtUtc,
            payment.Status,
            payment.GatewayStatus,
            payment.GatewayTransactionId,
            payment.FailureReason);
    }
}

