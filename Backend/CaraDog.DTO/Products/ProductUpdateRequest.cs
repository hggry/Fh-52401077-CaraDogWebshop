namespace CaraDog.DTO.Products;

public sealed record ProductUpdateRequest(
    string Name,
    string? Description,
    string Sku,
    decimal NetPrice,
    Guid CategoryId
);
