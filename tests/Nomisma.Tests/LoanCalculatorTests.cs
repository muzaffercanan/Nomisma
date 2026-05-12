using Nomisma.Domain.Loans;

namespace Nomisma.Tests;

public sealed class LoanCalculatorTests
{
    [Fact]
    public void Calculate_CreatesExpectedTotalDebt()
    {
        var result = new LoanCalculator().Calculate(10000m, 12m, 10, new DateOnly(2026, 1, 1));

        Assert.Equal(1200m, result.TotalProfit);
        Assert.Equal(11200m, result.TotalDebt);
        Assert.Equal(10, result.Installments.Count);
    }

    [Fact]
    public void Calculate_InstallmentsSumToTotalDebt()
    {
        var result = new LoanCalculator().Calculate(1000m, 10m, 3, new DateOnly(2026, 1, 1));

        Assert.Equal(result.TotalDebt, result.Installments.Sum(item => item.Amount));
        Assert.Equal(366.67m, result.Installments[0].Amount);
        Assert.Equal(366.66m, result.Installments[2].Amount);
    }

    [Fact]
    public void Calculate_RejectsInvalidPrincipal()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LoanCalculator().Calculate(0m, 10m, 12, new DateOnly(2026, 1, 1)));
    }
}
