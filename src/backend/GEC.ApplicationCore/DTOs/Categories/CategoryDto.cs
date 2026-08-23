namespace GEC.ApplicationCore.DTOs.Categories;

public record CategoryDto(
    Guid Id,
    Guid? ParentId,
    string Name,
    string Slug,
    bool IsActive
    );