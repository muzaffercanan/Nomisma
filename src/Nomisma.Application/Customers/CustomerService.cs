using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Nomisma.Application.Abstractions.Auth;
using Nomisma.Application.Abstractions.Persistence;
using Nomisma.Application.Common.Exceptions;
using Nomisma.Application.Common.Validation;
using Nomisma.Domain.Entities;
using Nomisma.Domain.Enums;

namespace Nomisma.Application.Customers;

public sealed class CustomerService : ICustomerService
{
    private readonly INomismaDbContext _dbContext;
    private readonly IUserAccountService _userAccountService;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<CreateCustomerRequestDto> _createValidator;
    private readonly IValidator<UpdateCustomerRequestDto> _updateValidator;

    public CustomerService(
        INomismaDbContext dbContext,
        IUserAccountService userAccountService,
        ICurrentUserService currentUser,
        IValidator<CreateCustomerRequestDto> createValidator,
        IValidator<UpdateCustomerRequestDto> updateValidator)
    {
        _dbContext = dbContext;
        _userAccountService = userAccountService;
        _currentUser = currentUser;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<CustomerResponseDto>> ListAsync(CancellationToken cancellationToken)
    {
        var customers = await _dbContext.Customers
            .AsNoTracking()
            .OrderBy(customer => customer.CustomerNumber)
            .ToListAsync(cancellationToken);

        return customers.Select(CustomerMapper.ToDto).ToList();
    }

    public async Task<CustomerResponseDto> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return customer is null
            ? throw new NotFoundException("Musteri bulunamadi.")
            : CustomerMapper.ToDto(customer);
    }

    public async Task<CustomerResponseDto> CreateAsync(CreateCustomerRequestDto request, CancellationToken cancellationToken)
    {
        await _createValidator.ValidateAndThrowApplicationExceptionAsync(request, cancellationToken);
        await EnsureUniqueAsync(request.NationalId, request.Email, null, cancellationToken);

        var customer = new Customer
        {
            CustomerNumber = await GenerateCustomerNumberAsync(cancellationToken),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            NationalId = request.NationalId.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PhoneNumber = request.PhoneNumber.Trim(),
            Address = request.Address.Trim(),
            DateOfBirth = request.DateOfBirth
        };

        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _userAccountService.CreateCustomerUserAsync(customer.Email, request.Password, customer.Id, cancellationToken);

        return CustomerMapper.ToDto(customer);
    }

    public async Task<CustomerResponseDto> UpdateAsync(Guid id, UpdateCustomerRequestDto request, CancellationToken cancellationToken)
    {
        await _updateValidator.ValidateAndThrowApplicationExceptionAsync(request, cancellationToken);
        var customer = await _dbContext.Customers.FirstOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new NotFoundException("Musteri bulunamadi.");

        var email = request.Email.Trim().ToLowerInvariant();
        await EnsureUniqueAsync(customer.NationalId, email, id, cancellationToken);

        customer.FirstName = request.FirstName.Trim();
        customer.LastName = request.LastName.Trim();
        customer.Email = email;
        customer.PhoneNumber = request.PhoneNumber.Trim();
        customer.Address = request.Address.Trim();
        customer.DateOfBirth = request.DateOfBirth;
        customer.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _userAccountService.UpdateCustomerEmailAsync(customer.Id, customer.Email, cancellationToken);

        return CustomerMapper.ToDto(customer);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers
            .Include(item => item.Loans)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new NotFoundException("Musteri bulunamadi.");

        if (customer.Loans.Any(loan => loan.Status == LoanStatus.Active))
        {
            throw new ConflictException("Aktif kredisi olan musteri silinemez.");
        }

        customer.IsDeleted = true;
        customer.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _userAccountService.DisableCustomerUserAsync(customer.Id, cancellationToken);
    }

    public async Task<CustomerSummaryResponseDto> GetSummaryAsync(Guid id, CancellationToken cancellationToken)
    {
        EnsureCanAccess(id);
        await MarkOverdueInstallmentsAsync(id, cancellationToken);

        var customer = await _dbContext.Customers
            .AsNoTracking()
            .Include(item => item.Loans)
            .ThenInclude(item => item.Installments)
            .ThenInclude(item => item.Payment)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new NotFoundException("Musteri bulunamadi.");

        return CustomerMapper.ToSummaryDto(customer);
    }

    public async Task<CustomerSummaryResponseDto> GetMySummaryAsync(CancellationToken cancellationToken)
    {
        var customerId = _currentUser.CustomerId
            ?? throw new ForbiddenException("Musteri baglantisi bulunamadi.");

        return await GetSummaryAsync(customerId, cancellationToken);
    }

    private async Task MarkOverdueInstallmentsAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var overdueInstallments = await _dbContext.Installments
            .Where(item =>
                item.Loan != null
                && item.Loan.CustomerId == customerId
                && item.Status == InstallmentStatus.Unpaid
                && item.DueDate < today)
            .ToListAsync(cancellationToken);

        if (overdueInstallments.Count == 0)
        {
            return;
        }

        foreach (var installment in overdueInstallments)
        {
            installment.Status = InstallmentStatus.Overdue;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
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

        throw new ForbiddenException("Bu musteri kaydina erisim yetkiniz yok.");
    }

    private async Task<string> GenerateCustomerNumberAsync(CancellationToken cancellationToken)
    {
        var count = await _dbContext.Customers.IgnoreQueryFilters().CountAsync(cancellationToken);
        return $"CUST-{count + 1001:D4}";
    }

    private async Task EnsureUniqueAsync(string nationalId, string email, Guid? currentCustomerId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Customers
            .IgnoreQueryFilters()
            .AnyAsync(customer =>
                customer.Id != currentCustomerId
                && (customer.NationalId == nationalId.Trim() || customer.Email == email.Trim().ToLowerInvariant()),
                cancellationToken);

        if (exists)
        {
            throw new ConflictException("Musteri kimlik numarasi veya email zaten kullaniliyor.");
        }
    }

}
