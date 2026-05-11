namespace Nomisma.Application.Abstractions.Auth;

public interface IJwtTokenService
{
    string CreateToken(JwtUserInfo user);
}

