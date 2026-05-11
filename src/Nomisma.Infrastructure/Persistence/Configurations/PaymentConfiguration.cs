using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nomisma.Domain.Entities;

namespace Nomisma.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(payment => payment.Id);
        builder.HasQueryFilter(payment =>
            payment.Installment != null
            && payment.Installment.Loan != null
            && payment.Installment.Loan.Customer != null
            && !payment.Installment.Loan.Customer.IsDeleted);

        builder.Property(payment => payment.Amount).HasColumnType("decimal(18,2)");
        builder.Property(payment => payment.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(payment => payment.GatewayStatus).HasConversion<string>().HasMaxLength(32);
        builder.Property(payment => payment.GatewayTransactionId).HasMaxLength(100).IsRequired();
        builder.Property(payment => payment.FailureReason).HasMaxLength(500);

        builder.HasIndex(payment => payment.InstallmentId).IsUnique();
        builder.HasIndex(payment => payment.GatewayTransactionId).IsUnique();
    }
}
