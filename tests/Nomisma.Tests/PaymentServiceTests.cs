using System.Data;
using Microsoft.EntityFrameworkCore;
using Nomisma.Application.Abstractions.Auth;
using Nomisma.Application.Abstractions.Integrations;
using Nomisma.Application.Abstractions.Persistence;
using Nomisma.Application.Common.Exceptions;
using Nomisma.Application.Payments;
using Nomisma.Domain.Entities;
using Nomisma.Domain.Enums;
using Nomisma.Infrastructure.Persistence;

namespace Nomisma.Tests;

public sealed class PaymentServiceTests
{
    [Fact]
    public async Task CreateAsync_GatewaySuccess_CreatesPaymentAndMarksInstallmentPaid()
    {
        await using var dbContext = CreateDbContext();
        var installment = await SeedLoanAsync(dbContext, installmentCount: 2);
        var service = CreateService(dbContext, PaymentGatewayResultFactory.Approved("TX-1"));

        var payment = await service.CreateAsync(CreateRequest(installment.Id, installment.Amount), CancellationToken.None);

        var paidInstallment = await dbContext.Installments.Include(item => item.Payment).FirstAsync(item => item.Id == installment.Id);
        Assert.Equal(installment.Amount, payment.Amount);
        Assert.Equal(InstallmentStatus.Paid, paidInstallment.Status);
        Assert.NotNull(paidInstallment.Payment);
    }

    [Fact]
    public async Task CreateAsync_GatewayFailure_DoesNotCreatePayment()
    {
        await using var dbContext = CreateDbContext();
        var installment = await SeedLoanAsync(dbContext, installmentCount: 2);
        var service = CreateService(dbContext, PaymentGatewayResultFactory.Declined("TX-DECLINED", "Rejected"));

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(CreateRequest(installment.Id, installment.Amount), CancellationToken.None));

        Assert.Empty(await dbContext.Payments.ToListAsync());
        var unchanged = await dbContext.Installments.FirstAsync(item => item.Id == installment.Id);
        Assert.Equal(InstallmentStatus.Unpaid, unchanged.Status);
    }

    [Fact]
    public async Task CreateAsync_RejectsPartialOrOverPayment()
    {
        await using var dbContext = CreateDbContext();
        var installment = await SeedLoanAsync(dbContext, installmentCount: 2);
        var service = CreateService(dbContext, PaymentGatewayResultFactory.Approved("TX-1"));

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.CreateAsync(CreateRequest(installment.Id, installment.Amount - 1m), CancellationToken.None));

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.CreateAsync(CreateRequest(installment.Id, installment.Amount + 1m), CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_WhenPreviousInstallmentUnpaid_RejectsPayment()
    {
        await using var dbContext = CreateDbContext();
        await SeedLoanAsync(dbContext, installmentCount: 2);
        var secondInstallment = await dbContext.Installments
            .OrderBy(item => item.InstallmentNumber)
            .Skip(1)
            .FirstAsync();
        var service = CreateService(dbContext, PaymentGatewayResultFactory.Approved("TX-1"));

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(CreateRequest(secondInstallment.Id, secondInstallment.Amount), CancellationToken.None));

        Assert.Empty(await dbContext.Payments.ToListAsync());
    }

    [Fact]
    public async Task CreateAsync_WhenEarlierCustomerInstallmentUnpaid_RejectsPayment()
    {
        await using var dbContext = CreateDbContext();
        var laterInstallment = await SeedTwoLoansForSameCustomerAsync(dbContext);
        var service = CreateService(dbContext, PaymentGatewayResultFactory.Approved("TX-1"));

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(CreateRequest(laterInstallment.Id, laterInstallment.Amount), CancellationToken.None));

        Assert.Empty(await dbContext.Payments.ToListAsync());
    }

    [Fact]
    public async Task CreateAsync_LastInstallmentPayment_ClosesLoan()
    {
        await using var dbContext = CreateDbContext();
        var installment = await SeedLoanAsync(dbContext, installmentCount: 1);
        var service = CreateService(dbContext, PaymentGatewayResultFactory.Approved("TX-1"));

        await service.CreateAsync(CreateRequest(installment.Id, installment.Amount), CancellationToken.None);

        var loan = await dbContext.Loans.FirstAsync();
        Assert.Equal(LoanStatus.Closed, loan.Status);
        Assert.NotNull(loan.ClosedAtUtc);
    }

    private static NomismaDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<NomismaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new NomismaDbContext(options);
    }

    private static PaymentService CreateService(NomismaDbContext dbContext, PaymentGatewayResultDto gatewayResult)
    {
        return new PaymentService(
            dbContext,
            new TestCurrentUserService(),
            new TestPaymentGateway(gatewayResult),
            new InlineTransactionManager(),
            new CreatePaymentRequestDtoValidator());
    }

    private static async Task<Installment> SeedLoanAsync(NomismaDbContext dbContext, int installmentCount)
    {
        var customer = new Customer
        {
            CustomerNumber = "CUST-TEST",
            FirstName = "Test",
            LastName = "Customer",
            NationalId = "10000000001",
            Email = "customer@nomisma.local",
            PhoneNumber = "+90 555 000 0000",
            Address = "Istanbul",
            DateOfBirth = new DateOnly(1990, 1, 1)
        };

        var loan = new Loan
        {
            CustomerId = customer.Id,
            Type = LoanType.Personal,
            PrincipalAmount = 100m * installmentCount,
            ProfitRate = 0m,
            TermMonths = installmentCount,
            StartDate = new DateOnly(2026, 1, 1),
            CreditScore = 750,
            TotalDebt = 100m * installmentCount,
            TotalProfit = 0m,
            Status = LoanStatus.Active
        };

        for (var number = 1; number <= installmentCount; number++)
        {
            loan.Installments.Add(new Installment
            {
                InstallmentNumber = number,
                PrincipalAmount = 100m,
                ProfitAmount = 0m,
                Amount = 100m,
                DueDate = new DateOnly(2026, 1, 1).AddMonths(number),
                Status = InstallmentStatus.Unpaid
            });
        }

        dbContext.Customers.Add(customer);
        dbContext.Loans.Add(loan);
        await dbContext.SaveChangesAsync();
        return await dbContext.Installments.OrderBy(item => item.InstallmentNumber).FirstAsync();
    }

    private static async Task<Installment> SeedTwoLoansForSameCustomerAsync(NomismaDbContext dbContext)
    {
        var customer = new Customer
        {
            CustomerNumber = "CUST-TEST",
            FirstName = "Test",
            LastName = "Customer",
            NationalId = "10000000001",
            Email = "customer@nomisma.local",
            PhoneNumber = "+90 555 000 0000",
            Address = "Istanbul",
            DateOfBirth = new DateOnly(1990, 1, 1)
        };

        var earlierLoan = CreateLoan(customer.Id, new DateOnly(2026, 1, 1));
        earlierLoan.Installments.Add(CreateInstallment(1, new DateOnly(2026, 2, 1)));

        var laterLoan = CreateLoan(customer.Id, new DateOnly(2026, 4, 1));
        laterLoan.Installments.Add(CreateInstallment(1, new DateOnly(2026, 5, 1)));

        dbContext.Customers.Add(customer);
        dbContext.Loans.AddRange(earlierLoan, laterLoan);
        await dbContext.SaveChangesAsync();

        return await dbContext.Installments
            .OrderByDescending(item => item.DueDate)
            .FirstAsync();
    }

    private static Loan CreateLoan(Guid customerId, DateOnly startDate)
    {
        return new Loan
        {
            CustomerId = customerId,
            Type = LoanType.Personal,
            PrincipalAmount = 100m,
            ProfitRate = 0m,
            TermMonths = 1,
            StartDate = startDate,
            CreditScore = 750,
            TotalDebt = 100m,
            TotalProfit = 0m,
            Status = LoanStatus.Active
        };
    }

    private static Installment CreateInstallment(int installmentNumber, DateOnly dueDate)
    {
        return new Installment
        {
            InstallmentNumber = installmentNumber,
            PrincipalAmount = 100m,
            ProfitAmount = 0m,
            Amount = 100m,
            DueDate = dueDate,
            Status = InstallmentStatus.Unpaid
        };
    }

    private static CreatePaymentRequestDto CreateRequest(Guid installmentId, decimal amount)
    {
        return new CreatePaymentRequestDto(
            installmentId,
            amount,
            "Test Customer",
            "4111111111111111",
            "123",
            12,
            2030);
    }

    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public Guid? UserId => Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        public Guid? CustomerId => null;
        public bool IsAdmin => true;
        public bool IsCustomer => false;
    }

    private sealed class TestPaymentGateway : IPaymentGateway
    {
        private readonly PaymentGatewayResultDto _result;

        public TestPaymentGateway(PaymentGatewayResultDto result)
        {
            _result = result;
        }

        public Task<PaymentGatewayResultDto> AuthorizeAsync(PaymentGatewayRequestDto request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_result);
        }
    }

    private sealed class InlineTransactionManager : ITransactionManager
    {
        public Task<T> ExecuteAsync<T>(
            Func<Task<T>> operation,
            CancellationToken cancellationToken,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return operation();
        }
    }
}

internal static class PaymentGatewayResultFactory
{
    public static PaymentGatewayResultDto Approved(string transactionId)
    {
        return new PaymentGatewayResultDto(true, GatewayStatus.Approved, transactionId, null);
    }

    public static PaymentGatewayResultDto Declined(string transactionId, string reason)
    {
        return new PaymentGatewayResultDto(false, GatewayStatus.Declined, transactionId, reason);
    }
}
