using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nomisma.Domain.Entities;
using Nomisma.Domain.Enums;
using Nomisma.Domain.Loans;
using Nomisma.Infrastructure.Identity;

namespace Nomisma.Infrastructure.Persistence;

public sealed class NomismaDbSeeder
{
    private const string AdminEmail = "admin@nomisma.local";
    private const string CustomerEmail = "customer@nomisma.local";
    private const string AdminPassword = "Admin123!";
    private const string CustomerPassword = "Customer123!";

    private readonly NomismaDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public NomismaDbSeeder(
        NomismaDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.MigrateAsync(cancellationToken);
        await EnsureRolesAsync();
        await EnsureAdminAsync();
        await EnsureCustomerDemoAsync(cancellationToken);
    }

    private async Task EnsureRolesAsync()
    {
        foreach (var role in Enum.GetNames<UserRole>())
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                var result = await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
                EnsureIdentitySuccess(result, $"Could not create role '{role}'.");
            }
        }
    }

    private async Task EnsureAdminAsync()
    {
        var admin = await _userManager.FindByEmailAsync(AdminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                Email = AdminEmail,
                UserName = AdminEmail,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(admin, AdminPassword);
            EnsureIdentitySuccess(createResult, "Could not create seed admin user.");
        }

        await EnsureUserRoleAsync(admin, UserRole.Admin.ToString());
    }

    private async Task EnsureCustomerDemoAsync(CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers
            .IgnoreQueryFilters()
            .Include(item => item.Loans)
            .ThenInclude(loan => loan.Installments)
            .FirstOrDefaultAsync(item => item.Email == CustomerEmail, cancellationToken);

        if (customer is null)
        {
            customer = new Customer
            {
                CustomerNumber = "CUST-1001",
                FirstName = "Demo",
                LastName = "Customer",
                NationalId = "10000000001",
                Email = CustomerEmail,
                PhoneNumber = "+90 555 010 1001",
                Address = "Istanbul",
                DateOfBirth = new DateOnly(1990, 1, 1)
            };

            _dbContext.Customers.Add(customer);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var user = await _userManager.FindByEmailAsync(CustomerEmail);
        if (user is null)
        {
            user = new ApplicationUser
            {
                Email = CustomerEmail,
                UserName = CustomerEmail,
                EmailConfirmed = true,
                CustomerId = customer.Id
            };

            var createResult = await _userManager.CreateAsync(user, CustomerPassword);
            EnsureIdentitySuccess(createResult, "Could not create seed customer user.");
        }
        else if (user.CustomerId != customer.Id)
        {
            user.CustomerId = customer.Id;
            var updateResult = await _userManager.UpdateAsync(user);
            EnsureIdentitySuccess(updateResult, "Could not attach seed customer user to customer.");
        }

        await EnsureUserRoleAsync(user, UserRole.Customer.ToString());
        await EnsureDemoLoanAsync(customer, cancellationToken);
    }

    private async Task EnsureDemoLoanAsync(Customer customer, CancellationToken cancellationToken)
    {
        if (customer.Loans.Any())
        {
            return;
        }

        var calculator = new LoanCalculator();
        var calculation = calculator.Calculate(25000m, 18m, 12, new DateOnly(2026, 5, 1));
        var loan = new Loan
        {
            CustomerId = customer.Id,
            Type = LoanType.Personal,
            PrincipalAmount = 25000m,
            ProfitRate = 18m,
            TermMonths = 12,
            StartDate = new DateOnly(2026, 5, 1),
            CreditScore = 760,
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
                DueDate = item.DueDate
            });
        }

        _dbContext.Loans.Add(loan);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureUserRoleAsync(ApplicationUser user, string role)
    {
        if (!await _userManager.IsInRoleAsync(user, role))
        {
            var result = await _userManager.AddToRoleAsync(user, role);
            EnsureIdentitySuccess(result, $"Could not assign role '{role}' to '{user.Email}'.");
        }
    }

    private static void EnsureIdentitySuccess(IdentityResult result, string message)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"{message} {errors}");
    }
}

