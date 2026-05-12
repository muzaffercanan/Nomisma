using FluentValidation;

namespace Nomisma.Application.Customers;

public sealed class UpdateCustomerRequestDtoValidator : AbstractValidator<UpdateCustomerRequestDto>
{
    public UpdateCustomerRequestDtoValidator()
    {
        RuleFor(request => request.FirstName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Ad ve soyad zorunludur.");

        RuleFor(request => request.LastName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Ad ve soyad zorunludur.");

        RuleFor(request => request.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Gecerli bir email girilmelidir.")
            .EmailAddress()
            .WithMessage("Gecerli bir email girilmelidir.");

        RuleFor(request => request.PhoneNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Telefon numarasi zorunludur.");

        RuleFor(request => request.Address)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Adres zorunludur.");
    }
}
