namespace CaraDog.DTO.Products;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string? Description,
    string Sku,
    decimal NetPrice,
    decimal GrossPrice,
    decimal TaxRate,
    bool IsSoldOut,
    Guid CategoryId,
    string CategoryName,
    IReadOnlyList<string> Tags
);
