using System.ComponentModel.DataAnnotations;

namespace CaraDog.Db.Entities;

public sealed class Product
{
    public Guid Id { get; set; }

    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string? Description { get; set; }

    [MaxLength(64)]
    public string Sku { get; set; } = string.Empty;

    public decimal NetPrice { get; set; }

    public bool IsSoldOut { get; set; }

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public Inventory? Inventory { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
}
