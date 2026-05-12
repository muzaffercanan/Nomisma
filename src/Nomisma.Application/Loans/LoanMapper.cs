using Nomisma.Application.Installments;
using Nomisma.Domain.Entities;
using Nomisma.Domain.Enums;

namespace Nomisma.Application.Loans;

public static class LoanMapper
{
    public static LoanResponseDto ToDto(Loan loan)
    {
        RefreshOverdueStatuses(loan.Installments);
        var installments = loan.Installments
            .OrderBy(item => item.InstallmentNumber)
            .Select(InstallmentMapper.ToDto)
            .ToList();

        var paidAmount = installments
            .Where(item => item.Status == InstallmentStatus.Paid)
            .Sum(item => item.Amount);

        return new LoanResponseDto(
            loan.Id,
            loan.CustomerId,
            loan.Type,
            loan.PrincipalAmount,
            loan.ProfitRate,
            loan.TermMonths,
            loan.StartDate,
            loan.Status,
            loan.CreditScore,
            loan.TotalProfit,
            loan.TotalDebt,
            paidAmount,
            loan.TotalDebt - paidAmount,
            loan.CreatedAtUtc,
            loan.ClosedAtUtc,
            installments);
    }

    private static void RefreshOverdueStatuses(IEnumerable<Installment> installments)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        foreach (var installment in installments)
        {
            if (installment.Status == InstallmentStatus.Unpaid && installment.DueDate < today)
            {
                installment.Status = InstallmentStatus.Overdue;
            }
        }
    }
}
