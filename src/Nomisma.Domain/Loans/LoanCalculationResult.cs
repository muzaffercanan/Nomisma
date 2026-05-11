namespace Nomisma.Domain.Loans;

public sealed record LoanCalculationResult(
    decimal TotalProfit,
    decimal TotalDebt,
    IReadOnlyList<InstallmentScheduleItem> Installments);

