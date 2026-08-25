using GEC.ApplicationCore.DTOs;
using GEC.Domain.Enums;
using GEC.ApplicationCore.Enums;

namespace GEC.ApplicationCore.DTOs.Products;

public class ProductFilterParams
{
    public PaginationParams Pagination { get; set; } = new();

    public ProductSortBy SortBy { get; set; } = ProductSortBy.Name;
    public SortDirection SortOrder { get; set; } = SortDirection.Asc;

    public string? SearchTerm { get; set; }
    public ProductStatus? Status { get; set; }
}
