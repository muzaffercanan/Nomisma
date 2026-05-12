using FluentValidation;

namespace Nomisma.Application.Loans;

public sealed class UpdateLoanRequestDtoValidator : AbstractValidator<UpdateLoanRequestDto>
{
    public UpdateLoanRequestDtoValidator()
    {
        RuleFor(request => request.Status)
            .IsInEnum()
            .WithMessage("Kredi durumu gecersiz.");
    }
}
