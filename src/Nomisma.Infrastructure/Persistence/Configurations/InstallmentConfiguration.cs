using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nomisma.Domain.Entities;

namespace Nomisma.Infrastructure.Persistence.Configurations;

public sealed class InstallmentConfiguration : IEntityTypeConfiguration<Installment>
{
    public void Configure(EntityTypeBuilder<Installment> builder)
    {
        builder.ToTable("Installments");
        builder.HasKey(installment => installment.Id);
        builder.HasQueryFilter(installment => installment.Loan != null && installment.Loan.Customer != null && !installment.Loan.Customer.IsDeleted);

        builder.Property(installment => installment.PrincipalAmount).HasColumnType("decimal(18,2)");
        builder.Property(installment => installment.ProfitAmount).HasColumnType("decimal(18,2)");
        builder.Property(installment => installment.Amount).HasColumnType("decimal(18,2)");
        builder.Property(installment => installment.Status).HasConversion<string>().HasMaxLength(32);

        builder.HasIndex(installment => new { installment.LoanId, installment.InstallmentNumber }).IsUnique();

        builder
            .HasOne(installment => installment.Payment)
            .WithOne(payment => payment.Installment)
            .HasForeignKey<Payment>(payment => payment.InstallmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
