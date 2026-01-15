namespace CaraDog.Db.Entities;

public sealed class OrderItem
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitNetPrice { get; set; }
    public decimal LineNet { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotalGross { get; set; }
}
