namespace Nomisma.Application.Payments;

public sealed record CreatePaymentRequestDto(
    Guid InstallmentId,
    decimal Amount,
    string CardHolderName,
    string CardNumber,
    string Cvv,
    int ExpiryMonth,
    int ExpiryYear);

