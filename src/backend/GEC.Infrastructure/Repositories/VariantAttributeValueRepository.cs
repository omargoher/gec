using GEC.ApplicationCore.Interfaces.Repositories;
using GEC.Domain.Entities;
using GEC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GEC.Infrastructure.Repositories;

public class VariantAttributeValueRepository : IVariantAttributeValueRepository
{
    private readonly ApplicationDbContext _context;

    public VariantAttributeValueRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VariantAttributeValue?> GetAsync(Guid variantId, Guid attributeId, CancellationToken cancellationToken = default)
        => await _context.VariantAttributeValues
            .FirstOrDefaultAsync(v => v.VariantId == variantId && v.AttributeId == attributeId, cancellationToken);

    public async Task<bool> ExistsAsync(Guid variantId, Guid attributeId, CancellationToken cancellationToken = default)
        => await _context.VariantAttributeValues.AnyAsync(
            v => v.VariantId == variantId && v.AttributeId == attributeId, cancellationToken);

    public async Task<bool> ExistsForProductAttributeAsync(Guid productId, Guid attributeId, CancellationToken cancellationToken = default)
        => await _context.VariantAttributeValues.AnyAsync(
            v => v.ProductId == productId && v.AttributeId == attributeId, cancellationToken);

    public async Task<bool> ExistsByAttributeValueIdAsync(Guid attributeValueId, CancellationToken cancellationToken = default)
        => await _context.VariantAttributeValues.AnyAsync(v => v.AttributeValueId == attributeValueId, cancellationToken);

    public async Task<List<VariantAttributeValue>> GetByVariantIdAsync(Guid variantId, CancellationToken cancellationToken = default)
        => await _context.VariantAttributeValues
            .Where(v => v.VariantId == variantId)
            .ToListAsync(cancellationToken);

    public void Add(VariantAttributeValue variantAttributeValue)
        => _context.VariantAttributeValues.Add(variantAttributeValue);

    public void Remove(VariantAttributeValue variantAttributeValue)
        => _context.VariantAttributeValues.Remove(variantAttributeValue);
}
