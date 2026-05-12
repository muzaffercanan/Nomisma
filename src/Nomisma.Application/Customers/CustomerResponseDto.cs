namespace Nomisma.Application.Customers;

public sealed record CustomerResponseDto(
    Guid Id,
    string CustomerNumber,
    string FirstName,
    string LastName,
    string FullName,
    string NationalId,
    string Email,
    string PhoneNumber,
    string Address,
    DateOnly DateOfBirth,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);

