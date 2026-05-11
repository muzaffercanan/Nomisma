using Nomisma.Domain.Common;
using Nomisma.Domain.Enums;

namespace Nomisma.Domain.Entities;

public sealed class Installment : Entity
{
    public Guid LoanId { get; set; }
    public Loan? Loan { get; set; }
    public int InstallmentNumber { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal ProfitAmount { get; set; }
    public decimal Amount { get; set; }
    public DateOnly DueDate { get; set; }
    public InstallmentStatus Status { get; set; } = InstallmentStatus.Unpaid;
    public DateTimeOffset? PaidAtUtc { get; set; }

    public Payment? Payment { get; set; }
}

