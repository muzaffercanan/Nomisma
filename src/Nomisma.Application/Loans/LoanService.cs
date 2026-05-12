using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Nomisma.Application.Abstractions.Auth;
using Nomisma.Application.Abstractions.Integrations;
using Nomisma.Application.Abstractions.Persistence;
using Nomisma.Application.Common.Exceptions;
using Nomisma.Application.Common.Validation;
using Nomisma.Application.Installments;
using Nomisma.Domain.Entities;
using Nomisma.Domain.Enums;
using Nomisma.Domain.Loans;
using AppValidationException = Nomisma.Application.Common.Exceptions.ValidationException;

namespace Nomisma.Application.Loans;

public sealed class LoanService : ILoanService
{
    private readonly INomismaDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly ICreditScoreService _creditScoreService;
    private readonly IValidator<CreateLoanRequestDto> _createValidator;
    private readonly IValidator<UpdateLoanRequestDto> _updateValidator;
    private readonly LoanCalculator _loanCalculator = new();

    public LoanService(
        INomismaDbContext dbContext,
        ICurrentUserService currentUser,
        ICreditScoreService creditScoreService,
        IValidator<CreateLoanRequestDto> createValidator,
        IValidator<UpdateLoanRequestDto> updateValidator)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _creditScoreService = creditScoreService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<LoanResponseDto>> ListAsync(Guid? customerId, CancellationToken cancellationToken)
    {
        var query = _dbContext.Loans
            .AsNoTracking()
            .Include(loan => loan.Installments)
            .ThenInclude(installment => installment.Payment)
            .AsQueryable();

        if (_currentUser.IsCustomer)
        {
            customerId = _currentUser.CustomerId ?? throw new ForbiddenException("Musteri baglantisi bulunamadi.");
        }

        if (customerId.HasValue)
        {
            query = query.Where(loan => loan.CustomerId == customerId.Value);
        }

        var loans = await query
            .OrderByDescending(loan => loan.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return loans.Select(LoanMapper.ToDto).ToList();
    }

    public async Task<LoanResponseDto> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var loan = await _dbContext.Loans
            .AsNoTracking()
            .Include(item => item.Installments)
            .ThenInclude(item => item.Payment)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new NotFoundException("Kredi bulunamadi.");

        EnsureCanAccess(loan.CustomerId);
        return LoanMapper.ToDto(loan);
    }

    public async Task<LoanResponseDto> CreateAsync(CreateLoanRequestDto request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAdmin)
        {
            throw new ForbiddenException("Kredi olusturma islemi yalnizca admin rolune aciktir.");
        }

        await _createValidator.ValidateAndThrowApplicationExceptionAsync(request, cancellationToken);

        var customer = await _dbContext.Customers.FirstOrDefaultAsync(item => item.Id == request.CustomerId, cancellationToken)
            ?? throw new NotFoundException("Musteri bulunamadi.");

        var creditScore = await _creditScoreService.GetScoreAsync(customer, cancellationToken);
        if (creditScore < 650)
        {
            throw new AppValidationException($"Kredi skoru yetersiz. Skor: {creditScore}, minimum: 650.");
        }

        var calculation = _loanCalculator.Calculate(request.PrincipalAmount, request.ProfitRate, request.TermMonths, request.StartDate);
        var loan = new Loan
        {
            CustomerId = request.CustomerId,
            Type = request.Type,
            PrincipalAmount = request.PrincipalAmount,
            ProfitRate = request.ProfitRate,
            TermMonths = request.TermMonths,
            StartDate = request.StartDate,
            CreditScore = creditScore,
            TotalProfit = calculation.TotalProfit,
            TotalDebt = calculation.TotalDebt,
            Status = LoanStatus.Active
        };

        foreach (var item in calculation.Installments)
        {
            loan.Installments.Add(new Installment
            {
                InstallmentNumber = item.InstallmentNumber,
                PrincipalAmount = item.PrincipalAmount,
                ProfitAmount = item.ProfitAmount,
                Amount = item.Amount,
                DueDate = item.DueDate,
                Status = InstallmentStatus.Unpaid
            });
        }

        _dbContext.Loans.Add(loan);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetAsync(loan.Id, cancellationToken);
    }

    public async Task<LoanResponseDto> UpdateAsync(Guid id, UpdateLoanRequestDto request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAdmin)
        {
            throw new ForbiddenException("Kredi guncelleme islemi yalnizca admin rolune aciktir.");
        }

        await _updateValidator.ValidateAndThrowApplicationExceptionAsync(request, cancellationToken);

        var loan = await _dbContext.Loans
            .Include(item => item.Installments)
            .ThenInclude(item => item.Payment)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new NotFoundException("Kredi bulunamadi.");

        if (loan.Status == LoanStatus.Closed && request.Status == LoanStatus.Active)
        {
            throw new ConflictException("Kapatilmis kredi tekrar aktif hale getirilemez.");
        }

        if (request.Status == LoanStatus.Closed && loan.Installments.Any(item => item.Status != InstallmentStatus.Paid))
        {
            throw new ConflictException("Tum taksitler odenmeden kredi kapatilamaz.");
        }

        loan.Status = request.Status;
        loan.ClosedAtUtc = request.Status == LoanStatus.Closed ? DateTimeOffset.UtcNow : null;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return LoanMapper.ToDto(loan);
    }

    public async Task<IReadOnlyList<InstallmentResponseDto>> GetInstallmentsAsync(Guid loanId, CancellationToken cancellationToken)
    {
        var loan = await _dbContext.Loans
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == loanId, cancellationToken)
            ?? throw new NotFoundException("Kredi bulunamadi.");

        EnsureCanAccess(loan.CustomerId);

        return await _dbContext.Installments
            .AsNoTracking()
            .Include(item => item.Payment)
            .Where(item => item.LoanId == loanId)
            .OrderBy(item => item.InstallmentNumber)
            .Select(item => InstallmentMapper.ToDto(item))
            .ToListAsync(cancellationToken);
    }

    private void EnsureCanAccess(Guid customerId)
    {
        if (_currentUser.IsAdmin)
        {
            return;
        }

        if (_currentUser.IsCustomer && _currentUser.CustomerId == customerId)
        {
            return;
        }

        throw new ForbiddenException("Bu kayda erisim yetkiniz yok.");
    }

}
