using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nomisma.Domain.Entities;

namespace Nomisma.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(customer => customer.Id);
        builder.HasQueryFilter(customer => !customer.IsDeleted);

        builder.Property(customer => customer.CustomerNumber).HasMaxLength(32).IsRequired();
        builder.Property(customer => customer.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(customer => customer.LastName).HasMaxLength(100).IsRequired();
        builder.Property(customer => customer.NationalId).HasMaxLength(20).IsRequired();
        builder.Property(customer => customer.Email).HasMaxLength(256).IsRequired();
        builder.Property(customer => customer.PhoneNumber).HasMaxLength(32).IsRequired();
        builder.Property(customer => customer.Address).HasMaxLength(500).IsRequired();

        builder.Ignore(customer => customer.FullName);

        builder.HasIndex(customer => customer.CustomerNumber).IsUnique();
        builder.HasIndex(customer => customer.NationalId).IsUnique();
        builder.HasIndex(customer => customer.Email).IsUnique();

        builder
            .HasMany(customer => customer.Loans)
            .WithOne(loan => loan.Customer)
            .HasForeignKey(loan => loan.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

