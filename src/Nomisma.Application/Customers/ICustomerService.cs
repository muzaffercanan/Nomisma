namespace Nomisma.Application.Customers;

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerResponseDto>> ListAsync(CancellationToken cancellationToken);

    Task<CustomerResponseDto> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<CustomerResponseDto> CreateAsync(CreateCustomerRequestDto request, CancellationToken cancellationToken);

    Task<CustomerResponseDto> UpdateAsync(Guid id, UpdateCustomerRequestDto request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<CustomerSummaryResponseDto> GetSummaryAsync(Guid id, CancellationToken cancellationToken);

    Task<CustomerSummaryResponseDto> GetMySummaryAsync(CancellationToken cancellationToken);
}
