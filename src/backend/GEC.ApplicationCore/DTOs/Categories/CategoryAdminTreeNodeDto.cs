namespace GEC.ApplicationCore.DTOs.Categories;

public record CategoryAdminTreeNodeDto(
    Guid Id,
    Guid? ParentId,
    string Name,
    string Slug,
    string? Icon,
    string? ImageUrl,
    bool IsActive,
    List<CategoryAdminTreeNodeDto> Children);