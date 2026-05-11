using Microsoft.EntityFrameworkCore;
using Nomisma.Application.Abstractions.Auth;
using Nomisma.Application.Abstractions.Persistence;
using Nomisma.Application.Common.Exceptions;
using Nomisma.Domain.Entities;
using Nomisma.Domain.Enums;

namespace Nomisma.Application.Customers;

public sealed class CustomerService
{
    private readonly INomismaDbContext _dbContext;
    private readonly IUserAccountService _userAccountService;

    public CustomerService(INomismaDbContext dbContext, IUserAccountService userAccountService)
    {
        _dbContext = dbContext;
        _userAccountService = userAccountService;
    }

    public async Task<IReadOnlyList<CustomerDto>> ListAsync(CancellationToken cancellationToken)
    {
        var customers = await _dbContext.Customers
            .AsNoTracking()
            .OrderBy(customer => customer.CustomerNumber)
            .ToListAsync(cancellationToken);

        return customers.Select(Map).ToList();
    }

    public async Task<CustomerDto> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return customer is null
            ? throw new NotFoundException("Musteri bulunamadi.")
            : Map(customer);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        ValidateCreate(request);
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

        return Map(customer);
    }

    public async Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        ValidateUpdate(request);
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

        return Map(customer);
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

    private static void ValidateCreate(CreateCustomerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            throw new ValidationException("Sifre en az 8 karakter olmalidir.");
        }

        ValidateCommon(request.FirstName, request.LastName, request.Email, request.NationalId, request.PhoneNumber);
    }

    private static void ValidateUpdate(UpdateCustomerRequest request)
    {
        ValidateCommon(request.FirstName, request.LastName, request.Email, "10000000000", request.PhoneNumber);
    }

    private static void ValidateCommon(string firstName, string lastName, string email, string nationalId, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            throw new ValidationException("Ad ve soyad zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@', StringComparison.Ordinal))
        {
            throw new ValidationException("Gecerli bir email girilmelidir.");
        }

        if (string.IsNullOrWhiteSpace(nationalId) || nationalId.Trim().Length < 10)
        {
            throw new ValidationException("Kimlik numarasi en az 10 karakter olmalidir.");
        }

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new ValidationException("Telefon numarasi zorunludur.");
        }
    }

    private static CustomerDto Map(Customer customer) => new(
        customer.Id,
        customer.CustomerNumber,
        customer.FirstName,
        customer.LastName,
        customer.FullName,
        customer.NationalId,
        customer.Email,
        customer.PhoneNumber,
        customer.Address,
        customer.DateOfBirth,
        customer.CreatedAtUtc,
        customer.UpdatedAtUtc);
}
