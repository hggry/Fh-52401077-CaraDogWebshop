namespace CaraDog.DTO.Orders;

public sealed record OrderItemCreateRequest(
    Guid ProductId,
    int Quantity
);
