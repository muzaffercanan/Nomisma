using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nomisma.Domain.Entities;

namespace Nomisma.Infrastructure.Persistence.Configurations;

public sealed class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("Loans");
        builder.HasKey(loan => loan.Id);
        builder.HasQueryFilter(loan => loan.Customer != null && !loan.Customer.IsDeleted);

        builder.Property(loan => loan.Type).HasConversion<string>().HasMaxLength(32);
        builder.Property(loan => loan.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(loan => loan.PrincipalAmount).HasColumnType("decimal(18,2)");
        builder.Property(loan => loan.ProfitRate).HasColumnType("decimal(5,2)");
        builder.Property(loan => loan.TotalProfit).HasColumnType("decimal(18,2)");
        builder.Property(loan => loan.TotalDebt).HasColumnType("decimal(18,2)");

        builder
            .HasMany(loan => loan.Installments)
            .WithOne(installment => installment.Loan)
            .HasForeignKey(installment => installment.LoanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
