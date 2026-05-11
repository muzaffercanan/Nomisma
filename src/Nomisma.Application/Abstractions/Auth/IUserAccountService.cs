namespace Nomisma.Application.Abstractions.Auth;

public interface IUserAccountService
{
    Task CreateCustomerUserAsync(string email, string password, Guid customerId, CancellationToken cancellationToken);
    Task UpdateCustomerEmailAsync(Guid customerId, string email, CancellationToken cancellationToken);
    Task DisableCustomerUserAsync(Guid customerId, CancellationToken cancellationToken);
}

