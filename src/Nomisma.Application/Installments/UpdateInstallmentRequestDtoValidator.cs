using FluentValidation;
using Nomisma.Domain.Enums;

namespace Nomisma.Application.Installments;

public sealed class UpdateInstallmentRequestDtoValidator : AbstractValidator<UpdateInstallmentRequestDto>
{
    public UpdateInstallmentRequestDtoValidator()
    {
        RuleFor(request => request.DueDate)
            .NotEmpty()
            .WithMessage("Vade tarihi zorunludur.");

        RuleFor(request => request.Status)
            .IsInEnum()
            .WithMessage("Taksit durumu gecersiz.")
            .NotEqual(InstallmentStatus.Paid)
            .WithMessage("Taksit odendi durumuna yalnizca odeme islemiyle alinabilir.");
    }
}
