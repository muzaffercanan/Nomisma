using Microsoft.EntityFrameworkCore;
using Nomisma.Application.Abstractions.Auth;
using Nomisma.Application.Abstractions.Integrations;
using Nomisma.Application.Abstractions.Persistence;
using Nomisma.Application.Common.Exceptions;
using Nomisma.Application.Installments;
using Nomisma.Domain.Entities;
using Nomisma.Domain.Enums;
using Nomisma.Domain.Loans;

namespace Nomisma.Application.Loans;

public sealed class LoanService
{
    private readonly INomismaDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly ICreditScoreService _creditScoreService;
    private readonly LoanCalculator _loanCalculator = new();

    public LoanService(
        INomismaDbContext dbContext,
        ICurrentUserService currentUser,
        ICreditScoreService creditScoreService)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _creditScoreService = creditScoreService;
    }

    public async Task<IReadOnlyList<LoanDto>> ListAsync(Guid? customerId, CancellationToken cancellationToken)
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

        return loans.Select(Map).ToList();
    }

    public async Task<LoanDto> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var loan = await _dbContext.Loans
            .AsNoTracking()
            .Include(item => item.Installments)
            .ThenInclude(item => item.Payment)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new NotFoundException("Kredi bulunamadi.");

        EnsureCanAccess(loan.CustomerId);
        return Map(loan);
    }

    public async Task<LoanDto> CreateAsync(CreateLoanRequest request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAdmin)
        {
            throw new ForbiddenException("Kredi olusturma islemi yalnizca admin rolune aciktir.");
        }

        ValidateCreate(request);

        var customer = await _dbContext.Customers.FirstOrDefaultAsync(item => item.Id == request.CustomerId, cancellationToken)
            ?? throw new NotFoundException("Musteri bulunamadi.");

        var creditScore = await _creditScoreService.GetScoreAsync(customer, cancellationToken);
        if (creditScore < 650)
        {
            throw new ValidationException($"Kredi skoru yetersiz. Skor: {creditScore}, minimum: 650.");
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

    public async Task<LoanDto> UpdateAsync(Guid id, UpdateLoanRequest request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAdmin)
        {
            throw new ForbiddenException("Kredi guncelleme islemi yalnizca admin rolune aciktir.");
        }

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

        return Map(loan);
    }

    public async Task<IReadOnlyList<InstallmentDto>> GetInstallmentsAsync(Guid loanId, CancellationToken cancellationToken)
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
            .Select(item => InstallmentService.Map(item))
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

    private static void ValidateCreate(CreateLoanRequest request)
    {
        if (request.PrincipalAmount <= 0)
        {
            throw new ValidationException("Ana para tutari sifirdan buyuk olmalidir.");
        }

        if (request.ProfitRate < 0)
        {
            throw new ValidationException("Kar orani negatif olamaz.");
        }

        if (request.TermMonths <= 0)
        {
            throw new ValidationException("Vade en az 1 ay olmalidir.");
        }
    }

    public static LoanDto Map(Loan loan)
    {
        RefreshOverdueStatuses(loan.Installments);
        var installments = loan.Installments
            .OrderBy(item => item.InstallmentNumber)
            .Select(InstallmentService.Map)
            .ToList();

        var paidAmount = installments
            .Where(item => item.Status == InstallmentStatus.Paid)
            .Sum(item => item.Amount);

        return new LoanDto(
            loan.Id,
            loan.CustomerId,
            loan.Type,
            loan.PrincipalAmount,
            loan.ProfitRate,
            loan.TermMonths,
            loan.StartDate,
            loan.Status,
            loan.CreditScore,
            loan.TotalProfit,
            loan.TotalDebt,
            paidAmount,
            loan.TotalDebt - paidAmount,
            loan.CreatedAtUtc,
            loan.ClosedAtUtc,
            installments);
    }

    private static void RefreshOverdueStatuses(IEnumerable<Installment> installments)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        foreach (var installment in installments)
        {
            if (installment.Status == InstallmentStatus.Unpaid && installment.DueDate < today)
            {
                installment.Status = InstallmentStatus.Overdue;
            }
        }
    }
}
