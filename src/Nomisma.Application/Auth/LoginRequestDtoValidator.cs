using FluentValidation;

namespace Nomisma.Application.Auth;

public sealed class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(request => request.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Email zorunludur.")
            .EmailAddress()
            .WithMessage("Gecerli bir email girilmelidir.");

        RuleFor(request => request.Password)
            .NotEmpty()
            .WithMessage("Sifre zorunludur.");
    }
}
