using Nomisma.Domain.Enums;

namespace Nomisma.Application.Payments;

public sealed record PaymentResponseDto(
    Guid Id,
    Guid InstallmentId,
    Guid LoanId,
    Guid CustomerId,
    decimal Amount,
    DateTimeOffset PaidAtUtc,
    PaymentStatus Status,
    GatewayStatus GatewayStatus,
    string GatewayTransactionId,
    string? FailureReason);

