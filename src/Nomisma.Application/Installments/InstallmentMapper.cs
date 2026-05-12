using Nomisma.Domain.Entities;
using Nomisma.Domain.Enums;

namespace Nomisma.Application.Installments;

public static class InstallmentMapper
{
    public static InstallmentResponseDto ToDto(Installment installment)
    {
        RefreshOverdueStatus(installment);
        return new InstallmentResponseDto(
            installment.Id,
            installment.LoanId,
            installment.InstallmentNumber,
            installment.PrincipalAmount,
            installment.ProfitAmount,
            installment.Amount,
            installment.DueDate,
            installment.Status,
            installment.PaidAtUtc,
            installment.Payment is not null);
    }

    private static void RefreshOverdueStatus(Installment installment)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (installment.Status == InstallmentStatus.Unpaid && installment.DueDate < today)
        {
            installment.Status = InstallmentStatus.Overdue;
        }
    }
}
