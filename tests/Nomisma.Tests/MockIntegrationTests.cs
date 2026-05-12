using Nomisma.Domain.Entities;
using Nomisma.Domain.Enums;
using Nomisma.Infrastructure.Integrations;

namespace Nomisma.Tests;

public sealed class MockIntegrationTests
{
    [Fact]
    public async Task MockCreditScore_ReturnsLowScoreForRiskProfile()
    {
        var service = new MockCreditScoreService();
        var customer = new Customer
        {
            Email = "risk.customer@nomisma.local",
            NationalId = "10000000001"
        };

        var score = await service.GetScoreAsync(customer, CancellationToken.None);

        Assert.Equal(580, score);
    }

    [Fact]
    public async Task MockPaymentGateway_ApprovesSuccessCard()
    {
        var service = new MockPaymentGateway();

        var result = await service.AuthorizeAsync(new(
            Guid.NewGuid(),
            100m,
            "Demo Customer",
            "4111111111111111",
            "123",
            12,
            2030), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(GatewayStatus.Approved, result.Status);
        Assert.NotEmpty(result.TransactionId);
    }

    [Fact]
    public async Task MockPaymentGateway_DeclinesFailureCard()
    {
        var service = new MockPaymentGateway();

        var result = await service.AuthorizeAsync(new(
            Guid.NewGuid(),
            100m,
            "Demo Customer",
            "4000000000000002",
            "123",
            12,
            2030), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(GatewayStatus.Declined, result.Status);
    }
}

