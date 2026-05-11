using Microsoft.EntityFrameworkCore;
using Nomisma.Domain.Entities;

namespace Nomisma.Application.Abstractions.Persistence;

public interface INomismaDbContext
{
    DbSet<Customer> Customers { get; }
    DbSet<Loan> Loans { get; }
    DbSet<Installment> Installments { get; }
    DbSet<Payment> Payments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

