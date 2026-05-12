namespace Nomisma.Application.Auth;

public sealed record LoginResponseDto(
    string Token,
    string Email,
    Guid? CustomerId,
    IReadOnlyCollection<string> Roles);
