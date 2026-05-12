using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Nomisma.Application.Customers;
using Nomisma.Application.Installments;
using Nomisma.Application.Loans;
using Nomisma.Application.Payments;

namespace Nomisma.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ILoanService, LoanService>();
        services.AddScoped<IInstallmentService, InstallmentService>();
        services.AddScoped<IPaymentService, PaymentService>();

        return services;
    }
}
