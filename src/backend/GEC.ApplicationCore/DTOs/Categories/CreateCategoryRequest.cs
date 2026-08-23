namespace GEC.ApplicationCore.DTOs.Categories;

public record CreateCategoryRequest(
    Guid? ParentId,
    string Name,
    string Slug,
    string? Description,
    string? Icon,
    string? ImageUrl,
    bool IsActive);
