using FluentValidation;

namespace Nomisma.Application.Payments;

public sealed class CreatePaymentRequestDtoValidator : AbstractValidator<CreatePaymentRequestDto>
{
    public CreatePaymentRequestDtoValidator()
    {
        RuleFor(request => request.InstallmentId)
            .NotEmpty()
            .WithMessage("Taksit secimi zorunludur.");

        RuleFor(request => request.Amount)
            .GreaterThan(0)
            .WithMessage("Odeme tutari sifirdan buyuk olmalidir.");

        RuleFor(request => request.CardHolderName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Kart bilgileri zorunludur.");

        RuleFor(request => request.CardNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Kart bilgileri zorunludur.");

        RuleFor(request => request.Cvv)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Kart bilgileri zorunludur.");

        RuleFor(request => request.ExpiryMonth)
            .InclusiveBetween(1, 12)
            .WithMessage("Kart son kullanma tarihi gecersiz.");

        RuleFor(request => request.ExpiryYear)
            .GreaterThanOrEqualTo(_ => DateTime.UtcNow.Year)
            .WithMessage("Kart son kullanma tarihi gecersiz.");
    }
}
