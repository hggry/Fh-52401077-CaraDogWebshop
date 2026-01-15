namespace CaraDog.DTO.Categories;

public sealed record CategoryUpdateRequest(
    string Name,
    string? Description
);
