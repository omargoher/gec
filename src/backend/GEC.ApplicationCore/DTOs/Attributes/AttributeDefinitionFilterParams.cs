using GEC.ApplicationCore.DTOs;
using GEC.ApplicationCore.Enums;
using GEC.Domain.Enums;

namespace GEC.ApplicationCore.DTOs.Attributes;

public class AttributeDefinitionFilterParams
{
    public PaginationParams Pagination { get; set; } = new();

    public AttributeDefinitionSortBy SortBy { get; set; } = AttributeDefinitionSortBy.Name;
    public SortDirection SortOrder { get; set; } = SortDirection.Asc;

    public string? SearchTerm { get; set; }
}
