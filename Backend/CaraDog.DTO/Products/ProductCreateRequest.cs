namespace CaraDog.DTO.Products;

public sealed record ProductCreateRequest(
    string Name,
    string? Description,
    string Sku,
    decimal NetPrice,
    Guid CategoryId
);
