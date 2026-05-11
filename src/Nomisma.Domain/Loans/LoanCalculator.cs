namespace Nomisma.Domain.Loans;

public sealed class LoanCalculator
{
    public LoanCalculationResult Calculate(decimal principalAmount, decimal profitRate, int termMonths, DateOnly startDate)
    {
        if (principalAmount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(principalAmount), "Principal amount must be greater than zero.");
        }

        if (profitRate < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(profitRate), "Profit rate cannot be negative.");
        }

        if (termMonths <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(termMonths), "Term must be greater than zero.");
        }

        var totalProfit = RoundMoney(principalAmount * profitRate / 100m);
        var totalDebt = principalAmount + totalProfit;
        var standardPrincipal = RoundMoney(principalAmount / termMonths);
        var standardProfit = RoundMoney(totalProfit / termMonths);
        var standardAmount = RoundMoney(totalDebt / termMonths);

        var schedule = new List<InstallmentScheduleItem>(termMonths);
        decimal principalAllocated = 0m;
        decimal profitAllocated = 0m;
        decimal amountAllocated = 0m;

        for (var number = 1; number <= termMonths; number++)
        {
            var isLast = number == termMonths;
            var principal = isLast ? principalAmount - principalAllocated : standardPrincipal;
            var profit = isLast ? totalProfit - profitAllocated : standardProfit;
            var amount = isLast ? totalDebt - amountAllocated : standardAmount;

            principal = RoundMoney(principal);
            profit = RoundMoney(profit);
            amount = RoundMoney(amount);

            schedule.Add(new InstallmentScheduleItem(
                number,
                principal,
                profit,
                amount,
                startDate.AddMonths(number)));

            principalAllocated += principal;
            profitAllocated += profit;
            amountAllocated += amount;
        }

        return new LoanCalculationResult(totalProfit, totalDebt, schedule);
    }

    private static decimal RoundMoney(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}

