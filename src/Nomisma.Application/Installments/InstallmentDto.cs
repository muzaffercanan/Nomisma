using Nomisma.Domain.Enums;

namespace Nomisma.Application.Installments;

public sealed record InstallmentDto(
    Guid Id,
    Guid LoanId,
    int InstallmentNumber,
    decimal PrincipalAmount,
    decimal ProfitAmount,
    decimal Amount,
    DateOnly DueDate,
    InstallmentStatus Status,
    DateTimeOffset? PaidAtUtc,
    bool HasPayment);

