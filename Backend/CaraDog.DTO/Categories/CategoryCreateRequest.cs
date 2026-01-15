namespace CaraDog.DTO.Categories;

public sealed record CategoryCreateRequest(
    string Name,
    string? Description
);
