namespace CaraDog.DTO.Customers;

public sealed record CustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    DateTime CreatedAt
);
