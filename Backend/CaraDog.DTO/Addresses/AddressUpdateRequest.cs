namespace CaraDog.DTO.Addresses;

public sealed record AddressUpdateRequest(
    string Street,
    string City,
    string PostalCode,
    string CountryCode,
    string? State
);
