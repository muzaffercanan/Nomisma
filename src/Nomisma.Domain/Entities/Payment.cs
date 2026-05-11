using Nomisma.Domain.Common;
using Nomisma.Domain.Enums;

namespace Nomisma.Domain.Entities;

public sealed class Payment : Entity
{
    public Guid InstallmentId { get; set; }
    public Installment? Installment { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset PaidAtUtc { get; set; }
    public PaymentStatus Status { get; set; }
    public GatewayStatus GatewayStatus { get; set; }
    public string GatewayTransactionId { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

