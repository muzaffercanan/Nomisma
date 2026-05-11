namespace Nomisma.Application.Customers;

public sealed record UpdateCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Address,
    DateOnly DateOfBirth);

