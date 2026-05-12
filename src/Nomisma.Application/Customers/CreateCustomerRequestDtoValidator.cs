using FluentValidation;

namespace Nomisma.Application.Customers;

public sealed class CreateCustomerRequestDtoValidator : AbstractValidator<CreateCustomerRequestDto>
{
    public CreateCustomerRequestDtoValidator()
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

        RuleFor(request => request.NationalId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Kimlik numarasi en az 10 karakter olmalidir.")
            .MinimumLength(10)
            .WithMessage("Kimlik numarasi en az 10 karakter olmalidir.");

        RuleFor(request => request.PhoneNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Telefon numarasi zorunludur.");

        RuleFor(request => request.Address)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Adres zorunludur.");

        RuleFor(request => request.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Sifre en az 8 karakter olmalidir.")
            .MinimumLength(8)
            .WithMessage("Sifre en az 8 karakter olmalidir.");
    }
}
