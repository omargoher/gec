using GEC.ApplicationCore.DTOs;
using GEC.ApplicationCore.DTOs.Attributes;
using GEC.ApplicationCore.Interfaces.Repositories;
using GEC.Domain.Entities;
using GEC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using GEC.Infrastructure.Extensions;

namespace GEC.Infrastructure.Repositories;

public class AttributeDefinitionRepository : BaseRepository<AttributeDefinition>, IAttributeDefinitionRepository
{
    private readonly ApplicationDbContext _context;

    public AttributeDefinitionRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        => await _context.AttributeDefinitions.AnyAsync(
            a => a.Name.ToLower() == name.ToLower(), cancellationToken);

    public async Task<PagedResult<AttributeDefinitionResponse>> GetPagedAsync(AttributeDefinitionFilterParams filterParams, CancellationToken cancellationToken = default)
    {
        return await _context.AttributeDefinitions
            .AsNoTracking()
            .Search(filterParams.SearchTerm)
            .ApplySort(filterParams.SortBy, filterParams.SortOrder)
            .Select(a => new AttributeDefinitionResponse { Id = a.Id, Name = a.Name })
            .ToPagedListAsync(filterParams.Pagination.PageNumber, filterParams.Pagination.PageSize, cancellationToken);
    }
}
