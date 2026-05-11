using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nomisma.Application.Abstractions.Persistence;
using Nomisma.Domain.Entities;
using Nomisma.Infrastructure.Identity;

namespace Nomisma.Infrastructure.Persistence;

public sealed class NomismaDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>,
        INomismaDbContext
{
    public NomismaDbContext(DbContextOptions<NomismaDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<Installment> Installments => Set<Installment>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(NomismaDbContext).Assembly);
    }
}

