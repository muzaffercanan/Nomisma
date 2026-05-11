using Microsoft.Extensions.DependencyInjection;
using Nomisma.Application.Customers;
using Nomisma.Application.Installments;
using Nomisma.Application.Loans;

namespace Nomisma.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CustomerService>();
        services.AddScoped<LoanService>();
        services.AddScoped<InstallmentService>();

        return services;
    }
}

