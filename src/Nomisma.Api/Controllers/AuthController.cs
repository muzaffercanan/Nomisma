using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nomisma.Application.Abstractions.Auth;
using Nomisma.Infrastructure.Identity;

namespace Nomisma.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(UserManager<ApplicationUser> userManager, IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized(new { message = "Email veya sifre hatali." });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenService.CreateToken(new JwtUserInfo(
            user.Id,
            user.Email ?? request.Email,
            user.CustomerId,
            roles.ToArray()));

        return Ok(new LoginResponse(
            token,
            user.Email ?? request.Email,
            user.CustomerId,
            roles.ToArray()));
    }
}

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(
    string Token,
    string Email,
    Guid? CustomerId,
    IReadOnlyCollection<string> Roles);

