using FluentValidation;

namespace Nomisma.Application.Loans;

public sealed class CreateLoanRequestDtoValidator : AbstractValidator<CreateLoanRequestDto>
{
    public CreateLoanRequestDtoValidator()
    {
        RuleFor(request => request.CustomerId)
            .NotEmpty()
            .WithMessage("Musteri secimi zorunludur.");

        RuleFor(request => request.Type)
            .IsInEnum()
            .WithMessage("Kredi tipi gecersiz.");

        RuleFor(request => request.PrincipalAmount)
            .GreaterThan(0)
            .WithMessage("Ana para tutari sifirdan buyuk olmalidir.");

        RuleFor(request => request.ProfitRate)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Kar orani negatif olamaz.");

        RuleFor(request => request.TermMonths)
            .GreaterThan(0)
            .WithMessage("Vade en az 1 ay olmalidir.");
    }
}
