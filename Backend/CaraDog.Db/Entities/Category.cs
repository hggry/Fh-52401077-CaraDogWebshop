using System.ComponentModel.DataAnnotations;

namespace CaraDog.Db.Entities;

public sealed class Category
{
    public Guid Id { get; set; }

    [MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(512)]
    public string? Description { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
