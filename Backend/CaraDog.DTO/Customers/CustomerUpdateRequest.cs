namespace CaraDog.DTO.Customers;

public sealed record CustomerUpdateRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone
);
