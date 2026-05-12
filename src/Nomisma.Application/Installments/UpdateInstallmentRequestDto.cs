using Nomisma.Domain.Enums;

namespace Nomisma.Application.Installments;

public sealed record UpdateInstallmentRequestDto(DateOnly DueDate, InstallmentStatus Status);

