namespace CaraDog.DTO.Carts;

public sealed record CartInfoRequest(
    IReadOnlyList<CartItemRequest> Items
);
