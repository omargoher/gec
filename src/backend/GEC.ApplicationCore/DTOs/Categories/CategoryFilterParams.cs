using GEC.Domain.Enums;
using GEC.ApplicationCore.Enums;

namespace GEC.ApplicationCore.DTOs.Categories;

public class CategoryFilterParams
{
    public PaginationParams Pagination { get; set; } = new();

    public CategorySortBy SortBy { get; set; } = CategorySortBy.DisplayOrder;
    public SortDirection SortOrder { get; set; } = SortDirection.Desc;
    public CategorySearchBy SearchBy { get; set; } = CategorySearchBy.Name;

    public string? SearchTerm { get; set; }

    public bool? IsActive { get; set; }
    public Guid? ParentId { get; set; }
}