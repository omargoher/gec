using GEC.ApplicationCore.DTOs.Attributes;
using GEC.ApplicationCore.Interfaces.Repositories;
using GEC.Domain.Entities;
using GEC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GEC.Infrastructure.Repositories;

public class AttributeValueRepository : BaseRepository<AttributeValue>, IAttributeValueRepository
{
    private readonly ApplicationDbContext _context;

    public AttributeValueRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByAttributeIdAsync(Guid attributeId, CancellationToken cancellationToken = default)
        => await _context.AttributeValues.AnyAsync(v => v.AttributeId == attributeId, cancellationToken);

    public async Task<bool> ExistsAsync(Guid attributeId, string value, CancellationToken cancellationToken = default)
        => await _context.AttributeValues.AnyAsync(
            v => v.AttributeId == attributeId && v.Value.ToLower() == value.ToLower(), cancellationToken);

    public async Task<List<AttributeValueResponse>> GetByAttributeIdAsync(Guid attributeId, CancellationToken cancellationToken = default)
        => await _context.AttributeValues
            .AsNoTracking()
            .Where(v => v.AttributeId == attributeId)
            .OrderBy(v => v.Value)
            .Select(v => new AttributeValueResponse { Id = v.Id, AttributeId = v.AttributeId, Value = v.Value })
            .ToListAsync(cancellationToken);
}
