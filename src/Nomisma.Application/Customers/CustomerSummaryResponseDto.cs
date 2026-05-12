using Nomisma.Application.Installments;

namespace Nomisma.Application.Customers;

public sealed record CustomerSummaryResponseDto(
    Guid CustomerId,
    string CustomerNumber,
    string FullName,
    decimal TotalLoanDebt,
    decimal RemainingPrincipal,
    decimal RemainingDebt,
    int OverdueInstallmentCount,
    IReadOnlyList<InstallmentResponseDto> PaidInstallments,
    IReadOnlyList<InstallmentResponseDto> UnpaidInstallments);

