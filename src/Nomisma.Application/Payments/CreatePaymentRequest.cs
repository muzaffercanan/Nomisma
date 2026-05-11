namespace Nomisma.Application.Payments;

public sealed record CreatePaymentRequest(
    Guid InstallmentId,
    decimal Amount,
    string CardHolderName,
    string CardNumber,
    string Cvv,
    int ExpiryMonth,
    int ExpiryYear);

