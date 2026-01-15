namespace CaraDog.DTO.Categories;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string? Description
);
