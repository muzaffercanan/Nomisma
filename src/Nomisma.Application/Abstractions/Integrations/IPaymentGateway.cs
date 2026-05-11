namespace Nomisma.Application.Abstractions.Integrations;

public interface IPaymentGateway
{
    Task<PaymentGatewayResult> AuthorizeAsync(PaymentGatewayRequest request, CancellationToken cancellationToken);
}

