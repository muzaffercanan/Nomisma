namespace Nomisma.Application.Customers;

public sealed record CreateCustomerRequest(
    string FirstName,
    string LastName,
    string NationalId,
    string Email,
    string PhoneNumber,
    string Address,
    DateOnly DateOfBirth,
    string Password);

