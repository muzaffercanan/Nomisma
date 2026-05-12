using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Nomisma.Application.Abstractions.Auth;
using Nomisma.Infrastructure.Auth;

namespace Nomisma.Tests;

public sealed class JwtTokenServiceTests
{
    [Fact]
    public void CreateToken_IncludesRoleAndCustomerClaims()
    {
        var service = new JwtTokenService(Options.Create(new JwtOptions
        {
            Issuer = "Nomisma",
            Audience = "Nomisma.Client",
            Secret = "Nomisma-test-secret-with-enough-length",
            ExpiryMinutes = 30
        }));
        var customerId = Guid.NewGuid();

        var token = service.CreateToken(new JwtUserInfo(
            Guid.NewGuid(),
            "customer@nomisma.local",
            customerId,
            new[] { "Customer" }));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Contains(jwt.Claims, claim => claim.Type == "customer_id" && claim.Value == customerId.ToString());
        Assert.Contains(jwt.Claims, claim => claim.Type.EndsWith("/role", StringComparison.Ordinal) && claim.Value == "Customer");
    }
}

