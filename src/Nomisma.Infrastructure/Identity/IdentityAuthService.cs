using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Nomisma.Application.Abstractions.Auth;
using Nomisma.Application.Auth;
using Nomisma.Application.Common.Exceptions;
using Nomisma.Application.Common.Validation;

namespace Nomisma.Infrastructure.Identity;

public sealed class IdentityAuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IValidator<LoginRequestDto> _loginValidator;

    public IdentityAuthService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        IValidator<LoginRequestDto> loginValidator)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _loginValidator = loginValidator;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        await _loginValidator.ValidateAndThrowApplicationExceptionAsync(request, cancellationToken);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new UnauthorizedException("Email veya sifre hatali.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var roleArray = roles.ToArray();
        var email = user.Email ?? request.Email;
        var token = _jwtTokenService.CreateToken(new JwtUserInfoDto(
            user.Id,
            email,
            user.CustomerId,
            roleArray));

        return new LoginResponseDto(
            token,
            email,
            user.CustomerId,
            roleArray);
    }
}
