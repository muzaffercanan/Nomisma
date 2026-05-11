using Microsoft.EntityFrameworkCore;
using Nomisma.Application.Abstractions.Auth;
using Nomisma.Application.Abstractions.Persistence;
using Nomisma.Application.Common.Exceptions;
using Nomisma.Domain.Entities;
using Nomisma.Domain.Enums;

namespace Nomisma.Application.Installments;

public sealed class InstallmentService
{
    private readonly INomismaDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public InstallmentService(INomismaDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<InstallmentDto> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var installment = await _dbContext.Installments
            .AsNoTracking()
            .Include(item => item.Loan)
            .Include(item => item.Payment)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new NotFoundException("Taksit bulunamadi.");

        EnsureCanAccess(installment.Loan?.CustomerId);
        RefreshOverdueStatus(installment);
        return Map(installment);
    }

    public async Task<InstallmentDto> UpdateAsync(Guid id, UpdateInstallmentRequest request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAdmin)
        {
            throw new ForbiddenException("Taksit guncelleme islemi yalnizca admin rolune aciktir.");
        }

        var installment = await _dbContext.Installments
            .Include(item => item.Payment)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new NotFoundException("Taksit bulunamadi.");

        if (installment.Payment is not null || installment.Status == InstallmentStatus.Paid)
        {
            throw new ConflictException("Odenmis taksit guncellenemez.");
        }

        if (request.Status == InstallmentStatus.Paid)
        {
            throw new ValidationException("Taksit odendi durumuna yalnizca odeme islemiyle alinabilir.");
        }

        installment.DueDate = request.DueDate;
        installment.Status = request.Status;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(installment);
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

        throw new ForbiddenException("Bu takside erisim yetkiniz yok.");
    }

    public static InstallmentDto Map(Installment installment)
    {
        RefreshOverdueStatus(installment);
        return new InstallmentDto(
            installment.Id,
            installment.LoanId,
            installment.InstallmentNumber,
            installment.PrincipalAmount,
            installment.ProfitAmount,
            installment.Amount,
            installment.DueDate,
            installment.Status,
            installment.PaidAtUtc,
            installment.Payment is not null);
    }

    private static void RefreshOverdueStatus(Installment installment)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (installment.Status == InstallmentStatus.Unpaid && installment.DueDate < today)
        {
            installment.Status = InstallmentStatus.Overdue;
        }
    }
}

