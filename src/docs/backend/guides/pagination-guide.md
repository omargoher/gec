# Pagination & Querying Guide

Implementing sorting, searching, and filtering follows a consistent 4-step pattern.

---

## Step 1: Create the Sort Enum
Create a sort option enum in `src/backend/GEC.ApplicationCore/Enums/` named `<EntityName>SortBy.cs`:
```csharp
namespace GEC.ApplicationCore.Enums;

public enum UserSortBy
{
    FullName,
    CreatedAt,
    Email
}
```

---

## Step 2: Create the FilterParams Class
Create a request parameter DTO in `src/backend/GEC.ApplicationCore/DTOs/` named `<EntityName>FilterParams.cs`. This class encapsulates all filtering criteria, pagination parameters, and sorting options:
```csharp
using GEC.ApplicationCore.Enums;

namespace GEC.ApplicationCore.DTOs.Users;

public class UserFilterParams
{
    public PaginationParams Pagination { get; set; } = new();

    public UserSortBy SortBy { get; set; } = UserSortBy.CreatedAt;
    public SortDirection SortOrder { get; set; } = SortDirection.Desc;

    public string? SearchTerm { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public bool? IsActive { get; set; }
}
```

---

## Step 3: Create Query Extension Methods
Create a static class for IQueryable extension methods in `src/backend/GEC.Infrastructure/Extensions/` named `<EntityName>QueryExtensions.cs`. This encapsulates the building of database query filters:
```csharp
using GEC.Domain.Entities;
using GEC.ApplicationCore.Enums;

namespace GEC.Infrastructure.Extensions;

public static class UserQueryExtensions
{
    public static IQueryable<User> ApplySort(
        this IQueryable<User> query, 
        UserSortBy sortBy, 
        SortDirection sortOrder)
    {
        var isDescending = sortOrder == SortDirection.Desc;

        query = sortBy switch
        {
            UserSortBy.FullName => isDescending
                ? query.OrderByDescending(u => u.FullName)
                : query.OrderBy(u => u.FullName),

            UserSortBy.Email => isDescending
                ? query.OrderByDescending(u => u.Email)
                : query.OrderBy(u => u.Email),

            UserSortBy.CreatedAt => isDescending
                ? query.OrderByDescending(u => u.CreatedAt)
                : query.OrderBy(u => u.CreatedAt),

            _ => query.OrderByDescending(u => u.CreatedAt)
        };

        return query;
    }

    public static IQueryable<User> Search(
        this IQueryable<User> query, 
        string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var lowerSearchTerm = searchTerm.Trim().ToLower();

        return query.Where(u =>
            u.FullName.ToLower().Contains(lowerSearchTerm) ||
            u.Email.ToLower().Contains(lowerSearchTerm)
        );
    }
    
    public static IQueryable<User> FilterByCity(
        this IQueryable<User> query, 
        string? city)
    {
        if (string.IsNullOrWhiteSpace(city))
            return query;

        return query.Where(u => u.City == city);
    }

    public static IQueryable<User> FilterByActive(
        this IQueryable<User> query, 
        bool? isActive)
    {
        if (!isActive.HasValue)
            return query;

        return query.Where(u => u.IsActive == isActive.Value);
    }
}
```

---

## Step 4: Use the Extensions in the Repository
Execute the query inside your Repository implementation using the static extension methods, and call `.ToPagedListAsync(...)` to execute pagination and count the records:
```csharp
using GEC.ApplicationCore.DTOs;
using GEC.ApplicationCore.DTOs.Users;
using GEC.Domain.Entities;
using GEC.Infrastructure.Extensions;

namespace GEC.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    private readonly ApplicationDbContext _context;
    
    public UserRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<PagedResult<User>> GetAllAsync(UserFilterParams filterParams, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .Search(filterParams.SearchTerm)
            .FilterByCity(filterParams.City)
            .FilterByActive(filterParams.IsActive)
            .ApplySort(filterParams.SortBy, filterParams.SortOrder)
            .ToPagedListAsync(filterParams.Pagination.PageNumber, filterParams.Pagination.PageSize, cancellationToken);
    }
}
```

---

## Architectural Guidelines
*   **Encapsulation**: Query filtering logic belongs in `GEC.Infrastructure/Extensions` (since it depends on EF Core details), whereas DTOs and interfaces belong in `GEC.ApplicationCore`.
*   **Execution**: Query pagination uses deferred execution (`IQueryable`). The database is only queried once when `.ToPagedListAsync` is called (which performs the `CountAsync` and fetches only the items in the current page).
*   **Result Structure**: All paginated responses should wrap the underlying list in a `PagedResult<T>` structure to present pagination metadata.
