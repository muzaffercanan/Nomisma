namespace Nomisma.Application.Abstractions.Auth;

public sealed record JwtUserInfo(
    Guid UserId,
    string Email,
    Guid? CustomerId,
    IReadOnlyCollection<string> Roles);

