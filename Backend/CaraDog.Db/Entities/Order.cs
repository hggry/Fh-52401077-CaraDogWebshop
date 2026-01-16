using CaraDog.Db.Enums;

namespace CaraDog.Db.Entities;

public sealed class Order
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public OrderStatus Status { get; set; }

    public PaymentProvider PaymentProvider { get; set; }

    public decimal SubtotalNet { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TotalGross { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
