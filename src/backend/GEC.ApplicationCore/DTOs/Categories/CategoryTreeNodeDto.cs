namespace GEC.ApplicationCore.DTOs.Categories;

public record CategoryTreeNodeDto(
    Guid Id,
    Guid? ParentId,
    string Name,
    string Slug,
    string? Icon,
    List<CategoryTreeNodeDto> Children
);