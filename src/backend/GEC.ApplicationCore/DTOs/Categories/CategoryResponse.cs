namespace GEC.ApplicationCore.DTOs.Categories;

public record CategoryResponse(
    Guid Id,
    Guid? ParentId,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    string? Icon,
    List<BreadcrumbDto> Breadcrumbs,
    List<SubCategoryDto> SubCategories
);