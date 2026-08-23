using GEC.ApplicationCore.DTOs;
using GEC.ApplicationCore.DTOs.Attributes;
using GEC.ApplicationCore.Interfaces.Repositories;
using GEC.Domain.Entities;
using GEC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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

    public async Task<PagedResult<AttributeDefinitionResponse>> GetPagedAsync(GetAttributeDefinitionsRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.AttributeDefinitions.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(a => a.Name.ToLower().Contains(search));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(a => a.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AttributeDefinitionResponse { Id = a.Id, Name = a.Name })
            .ToListAsync(cancellationToken);

        return new PagedResult<AttributeDefinitionResponse>
        {
            Data = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalRecords = total
        };
    }
}
