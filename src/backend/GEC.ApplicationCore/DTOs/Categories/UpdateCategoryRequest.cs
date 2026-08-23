namespace GEC.ApplicationCore.DTOs.Categories;

public record UpdateCategoryRequest(
    string? Name,
    string? Slug,
    string? Description,
    string? Icon,
    string? ImageUrl,
    Guid? ParentId,
    bool? ClearParent,
    bool? IsActive);
