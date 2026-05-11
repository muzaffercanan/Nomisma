namespace Nomisma.Domain.Loans;

public sealed record InstallmentScheduleItem(
    int InstallmentNumber,
    decimal PrincipalAmount,
    decimal ProfitAmount,
    decimal Amount,
    DateOnly DueDate);

