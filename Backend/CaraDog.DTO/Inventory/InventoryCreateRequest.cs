namespace CaraDog.DTO.Inventory;

public sealed record InventoryCreateRequest(
    Guid ProductId,
    int Quantity
);
