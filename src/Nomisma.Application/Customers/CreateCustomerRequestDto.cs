namespace Nomisma.Application.Customers;

public sealed record CreateCustomerRequestDto(
    string FirstName,
    string LastName,
    string NationalId,
    string Email,
    string PhoneNumber,
    string Address,
    DateOnly DateOfBirth,
    string Password);

