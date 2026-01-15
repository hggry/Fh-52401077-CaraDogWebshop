namespace CaraDog.DTO.Inventory;

public sealed record InventoryDto(
    Guid Id,
    Guid ProductId,
    int Quantity,
    DateTime UpdatedAt
);
