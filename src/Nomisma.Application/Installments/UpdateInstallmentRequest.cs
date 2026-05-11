using Nomisma.Domain.Enums;

namespace Nomisma.Application.Installments;

public sealed record UpdateInstallmentRequest(DateOnly DueDate, InstallmentStatus Status);

