namespace CaraDog.DTO.Carts;

public sealed record CartItemRequest(
    string Sku,
    int Quantity
);
