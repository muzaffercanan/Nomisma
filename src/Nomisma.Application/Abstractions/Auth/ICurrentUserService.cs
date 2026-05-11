namespace Nomisma.Application.Abstractions.Auth;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? CustomerId { get; }
    bool IsAdmin { get; }
    bool IsCustomer { get; }
}

