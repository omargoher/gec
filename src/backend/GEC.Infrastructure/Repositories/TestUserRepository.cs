using GEC.ApplicationCore.DTOs;
using GEC.ApplicationCore.DTOs.TestUser;
using GEC.ApplicationCore.Interfaces.Repositories;
using GEC.Domain.Entities;
using GEC.Infrastructure.Extensions;
using GEC.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace GEC.Infrastructure.Repositories;

public class TestUserRepository : BaseRepository<TestUser>, ITestUserRepository
{
    private readonly ApplicationDbContext _context;
    public TestUserRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<PagedResult<TestUser>> GetAll(TestUserFilterParams filterParams)
    {
        return await _context.TestUsers
            .AsNoTracking()
            .Search(filterParams.SearchTerm)
            .FilterByGender(filterParams.Gender)
            .ApplySort(filterParams.SortBy, filterParams.SortOrder)
            .ToPagedListAsync(filterParams.Pagination.PageNumber, filterParams.Pagination.PageSize, CancellationToken.None);
    }

}