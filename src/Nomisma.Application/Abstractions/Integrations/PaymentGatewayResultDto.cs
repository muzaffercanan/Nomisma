using Nomisma.Domain.Enums;

namespace Nomisma.Application.Abstractions.Integrations;

public sealed record PaymentGatewayResultDto(
    bool Succeeded,
    GatewayStatus Status,
    string TransactionId,
    string? FailureReason);

