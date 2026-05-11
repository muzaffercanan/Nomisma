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
        services.AddScoped<CustomerService>();
        services.AddScoped<LoanService>();
        services.AddScoped<InstallmentService>();
        services.AddScoped<PaymentService>();

        return services;
    }
}
