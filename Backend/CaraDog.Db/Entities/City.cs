using System.ComponentModel.DataAnnotations;

namespace CaraDog.Db.Entities;

public sealed class City
{
    public Guid Id { get; set; }

    [MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(16)]
    public string PostalCode { get; set; } = string.Empty;

    [MaxLength(2)]
    public string CountryCode { get; set; } = string.Empty;

    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
