using Nomisma.Domain.Enums;

namespace Nomisma.Application.Loans;

public sealed record CreateLoanRequest(
    Guid CustomerId,
    LoanType Type,
    decimal PrincipalAmount,
    decimal ProfitRate,
    int TermMonths,
    DateOnly StartDate);

