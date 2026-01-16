namespace CaraDog.DTO.Customers;

public sealed record CustomerCreateRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string Street,
    string HouseNumber,
    string? AddressLine2,
    string CityName,
    string PostalCode,
    string CountryCode
);
