using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nomisma.Application.Abstractions.Auth;
using Nomisma.Infrastructure.Auth;
using Nomisma.Infrastructure.Identity;
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
        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        services.Configure<JwtOptions>(options =>
        {
            options.Issuer = jwtSection["Issuer"] ?? string.Empty;
            options.Audience = jwtSection["Audience"] ?? string.Empty;
            options.Secret = jwtSection["Secret"] ?? string.Empty;
            options.ExpiryMinutes = int.TryParse(jwtSection["ExpiryMinutes"], out var expiryMinutes)
                ? expiryMinutes
                : 120;
        });

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<NomismaDbContext>();

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IUserAccountService, UserAccountService>();
        services.AddScoped<NomismaDbSeeder>();

        return services;
    }
}
