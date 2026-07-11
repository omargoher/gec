using GEC.Domain.Enums;
using GEC.ApplicationCore.Enums;

namespace GEC.ApplicationCore.DTOs.TestUser;

public class TestUserFilterParams
{
    public PaginationParams Pagination { get; set; } = new();

    public TestUserSortBy SortBy { get; set; } = TestUserSortBy.CreatedAt;
    public SortDirection SortOrder { get; set; } = SortDirection.Desc;

    public string? SearchTerm { get; set; }

    public Gender? Gender { get; set; }
}