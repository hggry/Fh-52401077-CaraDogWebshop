namespace CaraDog.DTO.Addresses;

public sealed record AddressCreateRequest(
    string Street,
    string City,
    string PostalCode,
    string CountryCode,
    string? State
);
