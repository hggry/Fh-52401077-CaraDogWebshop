namespace CaraDog.DTO.Customers;

public sealed record CustomerCreateRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone
);
