namespace Nomisma.Application.Abstractions.Integrations;

public sealed record PaymentGatewayRequest(
    Guid InstallmentId,
    decimal Amount,
    string CardHolderName,
    string CardNumber,
    string Cvv,
    int ExpiryMonth,
    int ExpiryYear);

