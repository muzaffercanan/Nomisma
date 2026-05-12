namespace Nomisma.Application.Payments;

public interface IPaymentService
{
    Task<IReadOnlyList<PaymentResponseDto>> ListAsync(CancellationToken cancellationToken);

    Task<PaymentResponseDto> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<PaymentResponseDto> CreateAsync(CreatePaymentRequestDto request, CancellationToken cancellationToken);
}
