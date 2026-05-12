namespace Nomisma.Application.Abstractions.Auth;

public sealed record JwtUserInfoDto(
    Guid UserId,
    string Email,
    Guid? CustomerId,
    IReadOnlyCollection<string> Roles);

