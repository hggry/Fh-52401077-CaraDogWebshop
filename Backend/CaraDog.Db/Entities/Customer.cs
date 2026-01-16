using System.ComponentModel.DataAnnotations;

namespace CaraDog.Db.Entities;

public sealed class Customer
{
    public Guid Id { get; set; }

    [MaxLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(80)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(160)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(40)]
    public string? Phone { get; set; }

    [MaxLength(120)]
    public string Street { get; set; } = string.Empty;

    [MaxLength(16)]
    public string HouseNumber { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? AddressLine2 { get; set; }

    public Guid CityId { get; set; }
    public City City { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
