using System.Security.Claims;
using Nomisma.Application.Abstractions.Auth;
using Nomisma.Domain.Enums;

namespace Nomisma.Api.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId => TryParseGuid(FindValue(ClaimTypes.NameIdentifier));

    public Guid? CustomerId => TryParseGuid(FindValue("customer_id"));

    public bool IsAdmin => IsInRole(UserRole.Admin.ToString());

    public bool IsCustomer => IsInRole(UserRole.Customer.ToString());

    private bool IsInRole(string role) => _httpContextAccessor.HttpContext?.User.IsInRole(role) == true;

    private string? FindValue(string claimType) => _httpContextAccessor.HttpContext?.User.FindFirstValue(claimType);

    private static Guid? TryParseGuid(string? value)
    {
        return Guid.TryParse(value, out var id) ? id : null;
    }
}

