namespace Nomisma.Application.Installments;

public interface IInstallmentService
{
    Task<InstallmentResponseDto> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<InstallmentResponseDto> UpdateAsync(Guid id, UpdateInstallmentRequestDto request, CancellationToken cancellationToken);
}
