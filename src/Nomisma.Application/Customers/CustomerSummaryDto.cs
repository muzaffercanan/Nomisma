using Nomisma.Application.Installments;

namespace Nomisma.Application.Customers;

public sealed record CustomerSummaryDto(
    Guid CustomerId,
    string CustomerNumber,
    string FullName,
    decimal TotalLoanDebt,
    decimal RemainingPrincipal,
    decimal RemainingDebt,
    int OverdueInstallmentCount,
    IReadOnlyList<InstallmentDto> PaidInstallments,
    IReadOnlyList<InstallmentDto> UnpaidInstallments);

