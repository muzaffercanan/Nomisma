using Microsoft.Extensions.DependencyInjection;

namespace Nomisma.Infrastructure.Persistence;

public static class NomismaDbSeederExtensions
{
    public static async Task MigrateAndSeedNomismaAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<NomismaDbSeeder>();
        await seeder.SeedAsync();
    }
}

