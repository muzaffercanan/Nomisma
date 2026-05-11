using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nomisma.Infrastructure.Persistence;

public sealed class NomismaDbContextFactory : IDesignTimeDbContextFactory<NomismaDbContext>
{
    public NomismaDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<NomismaDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=NomismaDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True")
            .Options;

        return new NomismaDbContext(options);
    }
}

