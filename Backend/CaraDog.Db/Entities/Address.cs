using System.ComponentModel.DataAnnotations;

namespace CaraDog.Db.Entities;

public sealed class Address
{
    public Guid Id { get; set; }

    [MaxLength(120)]
    public string Street { get; set; } = string.Empty;

    [MaxLength(80)]
    public string City { get; set; } = string.Empty;

    [MaxLength(12)]
    public string PostalCode { get; set; } = string.Empty;

    [MaxLength(2)]
    public string CountryCode { get; set; } = string.Empty;

    [MaxLength(80)]
    public string? State { get; set; }

    
}
