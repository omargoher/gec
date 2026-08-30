using GEC.Domain.Entities;
using GEC.ApplicationCore.Enums;

namespace GEC.Infrastructure.Extensions;

public static class AttributeDefinitionQueryExtensions
{
    public static IQueryable<AttributeDefinition> ApplySort(
        this IQueryable<AttributeDefinition> query,
        AttributeDefinitionSortBy sortBy,
        SortDirection sortOrder)
    {
        var isDescending = sortOrder == SortDirection.Desc;

        query = sortBy switch
        {
            AttributeDefinitionSortBy.Name => isDescending
                ? query.OrderByDescending(a => a.Name)
                : query.OrderBy(a => a.Name),

            AttributeDefinitionSortBy.CreatedAt => isDescending
                ? query.OrderByDescending(a => a.CreatedAt)
                : query.OrderBy(a => a.CreatedAt),

            _ => query.OrderBy(a => a.Name)
        };

        return query;
    }

    public static IQueryable<AttributeDefinition> Search(
        this IQueryable<AttributeDefinition> query,
        string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var lowerSearchTerm = searchTerm.Trim().ToLower();

        return query.Where(a => a.Name.ToLower().Contains(lowerSearchTerm));
    }
}
