using GEC.ApplicationCore.Enums;
using GEC.Domain.Entities;
using GEC.Domain.Enums;

namespace GEC.Infrastructure.Extensions;

public static class TestUserQueryExtensions
{
    public static IQueryable<TestUser> ApplySort(
        this IQueryable<TestUser> query,
        TestUserSortBy sortBy,
        SortDirection sortOrder)
    {
        var isDescending = sortOrder == SortDirection.Desc;

        // Map sorting field names
        query = sortBy switch
        {
            TestUserSortBy.FullName => isDescending
                ? query.OrderByDescending(u => u.Name)
                : query.OrderBy(u => u.Name),

            TestUserSortBy.CreatedAt => isDescending
                ? query.OrderByDescending(u => u.CreatedAt)
                : query.OrderBy(u => u.CreatedAt),

            // Default: sort by CreatedAt desc
            _ => query.OrderByDescending(u => u.CreatedAt)
        };

        return query;
    }

    public static IQueryable<TestUser> Search(
        this IQueryable<TestUser> query,
        string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var lowerSearchTerm = searchTerm.Trim().ToLower();

        // may use it
        // return query.Where(u =>
        //     EF.Functions.ILike(u.FullName, $"%{searchTerm}%") ||
        //     EF.Functions.ILike(u.Email, $"%{searchTerm}%")
        // );
        return query.Where(u =>
            u.Name.ToLower().Contains(lowerSearchTerm)
        );
    }

    public static IQueryable<TestUser> FilterByGender(
        this IQueryable<TestUser> query,
        Gender? gender)
    {
        if (gender != null)
            query = query.Where(c => c.Gender == gender);

        return query;
    }
}