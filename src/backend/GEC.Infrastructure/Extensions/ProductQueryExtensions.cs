using GEC.Domain.Entities;
using GEC.ApplicationCore.Enums;
using GEC.Domain.Enums;

namespace GEC.Infrastructure.Extensions;

public static class ProductQueryExtensions
{
    public static IQueryable<Product> ApplySort(
        this IQueryable<Product> query,
        ProductSortBy sortBy,
        SortDirection sortOrder)
    {
        var isDescending = sortOrder == SortDirection.Desc;

        query = sortBy switch
        {
            ProductSortBy.Name => isDescending
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name),

            ProductSortBy.CreatedAt => isDescending
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt),

            _ => query.OrderBy(p => p.Name)
        };

        return query;
    }

    public static IQueryable<Product> Search(
        this IQueryable<Product> query,
        string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var lowerSearchTerm = searchTerm.Trim().ToLower();

        return query.Where(p =>
            p.Name.ToLower().Contains(lowerSearchTerm) ||
            p.Slug.ToLower().Contains(lowerSearchTerm)
        );
    }

    public static IQueryable<Product> FilterByStatus(
        this IQueryable<Product> query,
        ProductStatus? status)
    {
        if (!status.HasValue)
            return query;

        return query.Where(p => p.Status == status.Value);
    }

    public static IQueryable<Product> FilterByCategory(
        this IQueryable<Product> query,
        Guid? categoryId)
    {
        if (!categoryId.HasValue)
            return query;

        return query.Where(p => p.ProductCategories.Any(pc => pc.CategoryId == categoryId.Value));
    }
}
