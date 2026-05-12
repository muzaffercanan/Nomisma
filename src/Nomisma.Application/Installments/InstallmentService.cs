using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Nomisma.Application.Abstractions.Auth;
using Nomisma.Application.Abstractions.Persistence;
using Nomisma.Application.Common.Exceptions;
using Nomisma.Application.Common.Validation;
using Nomisma.Domain.Enums;

namespace Nomisma.Application.Installments;

public sealed class InstallmentService : IInstallmentService
{
    private readonly INomismaDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<UpdateInstallmentRequestDto> _updateValidator;

    public InstallmentService(
        INomismaDbContext dbContext,
        ICurrentUserService currentUser,
        IValidator<UpdateInstallmentRequestDto> updateValidator)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _updateValidator = updateValidator;
    }

    public async Task<InstallmentResponseDto> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var installment = await _dbContext.Installments
            .AsNoTracking()
            .Include(item => item.Loan)
            .Include(item => item.Payment)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new NotFoundException("Taksit bulunamadi.");

        EnsureCanAccess(installment.Loan?.CustomerId);
        return InstallmentMapper.ToDto(installment);
    }

    public async Task<InstallmentResponseDto> UpdateAsync(Guid id, UpdateInstallmentRequestDto request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAdmin)
        {
            throw new ForbiddenException("Taksit guncelleme islemi yalnizca admin rolune aciktir.");
        }

        await _updateValidator.ValidateAndThrowApplicationExceptionAsync(request, cancellationToken);

        var installment = await _dbContext.Installments
            .Include(item => item.Payment)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new NotFoundException("Taksit bulunamadi.");

        if (installment.Payment is not null || installment.Status == InstallmentStatus.Paid)
        {
            throw new ConflictException("Odenmis taksit guncellenemez.");
        }

        installment.DueDate = request.DueDate;
        installment.Status = request.Status;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return InstallmentMapper.ToDto(installment);
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

}
