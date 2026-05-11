using Nomisma.Domain.Common;
using Nomisma.Domain.Enums;

namespace Nomisma.Domain.Entities;

public sealed class Loan : Entity
{
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public LoanType Type { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal ProfitRate { get; set; }
    public int TermMonths { get; set; }
    public DateOnly StartDate { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Active;
    public int CreditScore { get; set; }
    public decimal TotalProfit { get; set; }
    public decimal TotalDebt { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ClosedAtUtc { get; set; }

    public ICollection<Installment> Installments { get; set; } = new List<Installment>();
}

