namespace GEC.ApplicationCore.DTOs.Categories;

public record SubCategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Icon,
    string? ImageUrl
    );