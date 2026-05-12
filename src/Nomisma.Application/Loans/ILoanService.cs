using Nomisma.Application.Installments;

namespace Nomisma.Application.Loans;

public interface ILoanService
{
    Task<IReadOnlyList<LoanResponseDto>> ListAsync(Guid? customerId, CancellationToken cancellationToken);

    Task<LoanResponseDto> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<LoanResponseDto> CreateAsync(CreateLoanRequestDto request, CancellationToken cancellationToken);

    Task<LoanResponseDto> UpdateAsync(Guid id, UpdateLoanRequestDto request, CancellationToken cancellationToken);

    Task<IReadOnlyList<InstallmentResponseDto>> GetInstallmentsAsync(Guid loanId, CancellationToken cancellationToken);
}
