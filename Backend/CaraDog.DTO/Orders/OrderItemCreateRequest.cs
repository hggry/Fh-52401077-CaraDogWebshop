namespace CaraDog.DTO.Orders;

public sealed record OrderItemCreateRequest(
    string Sku,
    int Quantity
);
