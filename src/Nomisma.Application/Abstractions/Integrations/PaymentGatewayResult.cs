using Nomisma.Domain.Enums;

namespace Nomisma.Application.Abstractions.Integrations;

public sealed record PaymentGatewayResult(
    bool Succeeded,
    GatewayStatus Status,
    string TransactionId,
    string? FailureReason);

