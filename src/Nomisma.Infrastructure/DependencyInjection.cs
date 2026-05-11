using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nomisma.Application.Abstractions.Persistence;
using Nomisma.Infrastructure.Persistence;

namespace Nomisma.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("NomismaDb")
            ?? throw new InvalidOperationException("Connection string 'NomismaDb' is not configured.");

        services.AddDbContext<NomismaDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<INomismaDbContext>(provider => provider.GetRequiredService<NomismaDbContext>());

        return services;
    }
}

