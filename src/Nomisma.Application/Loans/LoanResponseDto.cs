using Nomisma.Application.Installments;
using Nomisma.Domain.Enums;

namespace Nomisma.Application.Loans;

public sealed record LoanResponseDto(
    Guid Id,
    Guid CustomerId,
    LoanType Type,
    decimal PrincipalAmount,
    decimal ProfitRate,
    int TermMonths,
    DateOnly StartDate,
    LoanStatus Status,
    int CreditScore,
    decimal TotalProfit,
    decimal TotalDebt,
    decimal PaidAmount,
    decimal RemainingDebt,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ClosedAtUtc,
    IReadOnlyList<InstallmentResponseDto> Installments);

