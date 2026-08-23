using GEC.ApplicationCore.DTOs.Products;
using GEC.ApplicationCore.Interfaces.Repositories;
using GEC.Domain.Entities;
using GEC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GEC.Infrastructure.Repositories;

public class ProductAttributeRepository : IProductAttributeRepository
{
    private readonly ApplicationDbContext _context;

    public ProductAttributeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductAttribute?> GetAsync(Guid productId, Guid attributeId, CancellationToken cancellationToken = default)
        => await _context.ProductAttributes
            .Include(pa => pa.Attribute)
            .FirstOrDefaultAsync(pa => pa.ProductId == productId && pa.AttributeId == attributeId, cancellationToken);

    public async Task<bool> ExistsAsync(Guid productId, Guid attributeId, CancellationToken cancellationToken = default)
        => await _context.ProductAttributes.AnyAsync(
            pa => pa.ProductId == productId && pa.AttributeId == attributeId, cancellationToken);

    public async Task<bool> ExistsByAttributeIdAsync(Guid attributeId, CancellationToken cancellationToken = default)
        => await _context.ProductAttributes.AnyAsync(pa => pa.AttributeId == attributeId, cancellationToken);

    public async Task<List<ProductAttributeResponse>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
        => await _context.ProductAttributes
            .AsNoTracking()
            .Include(pa => pa.Attribute)
            .Where(pa => pa.ProductId == productId)
            .Select(pa => new ProductAttributeResponse
            {
                ProductId = pa.ProductId,
                AttributeId = pa.AttributeId,
                AttributeName = pa.Attribute.Name,
                IsRequired = pa.IsRequired
            })
            .ToListAsync(cancellationToken);

    public async Task<List<ProductAttribute>> GetRequiredByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
        => await _context.ProductAttributes
            .Where(pa => pa.ProductId == productId && pa.IsRequired)
            .ToListAsync(cancellationToken);

    public void Add(ProductAttribute productAttribute)
        => _context.ProductAttributes.Add(productAttribute);

    public void Remove(ProductAttribute productAttribute)
        => _context.ProductAttributes.Remove(productAttribute);
}
