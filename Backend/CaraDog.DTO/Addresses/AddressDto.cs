namespace CaraDog.DTO.Addresses;

public sealed record AddressDto(
    Guid Id,
    string Street,
    string City,
    string PostalCode,
    string CountryCode,
    string? State
);
