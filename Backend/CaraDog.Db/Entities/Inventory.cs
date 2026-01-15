namespace CaraDog.Db.Entities;

public sealed class Inventory
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }

    public DateTime UpdatedAt { get; set; }
}
