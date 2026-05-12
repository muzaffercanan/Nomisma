using Nomisma.Application.Abstractions.Integrations;
using Nomisma.Domain.Enums;

namespace Nomisma.Infrastructure.Integrations;

public sealed class MockPaymentGateway : IPaymentGateway
{
    public Task<PaymentGatewayResultDto> AuthorizeAsync(PaymentGatewayRequestDto request, CancellationToken cancellationToken)
    {
        var normalizedCard = new string(request.CardNumber.Where(char.IsDigit).ToArray());

        if (request.Amount <= 0)
        {
            return Declined("Gecersiz odeme tutari.");
        }

        if (normalizedCard is "4000000000000002" or "0000000000000000")
        {
            return Declined("Mock gateway odemeyi reddetti.");
        }

        if (normalizedCard.Length < 12 || string.IsNullOrWhiteSpace(request.Cvv))
        {
            return Declined("Kart bilgileri gecersiz.");
        }

        var transactionId = $"MOCK-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}"[..37];
        var result = new PaymentGatewayResultDto(true, GatewayStatus.Approved, transactionId, null);
        return Task.FromResult(result);
    }

    private static Task<PaymentGatewayResultDto> Declined(string reason)
    {
        var transactionId = $"DECLINED-{Guid.NewGuid():N}"[..37];
        var result = new PaymentGatewayResultDto(false, GatewayStatus.Declined, transactionId, reason);
        return Task.FromResult(result);
    }
}

