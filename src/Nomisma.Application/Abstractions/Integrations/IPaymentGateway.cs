namespace Nomisma.Application.Abstractions.Integrations;

public interface IPaymentGateway
{
    Task<PaymentGatewayResultDto> AuthorizeAsync(PaymentGatewayRequestDto request, CancellationToken cancellationToken);
}

