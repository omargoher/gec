using GEC.ApplicationCore.DTOs.Products;
using GEC.ApplicationCore.Interfaces.Repositories;
using GEC.Domain.Entities;
using GEC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GEC.Infrastructure.Repositories;

public class ProductVariantRepository : BaseRepository<ProductVariant>, IProductVariantRepository
{
    private readonly ApplicationDbContext _context;

    public ProductVariantRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> ExistsBySkuAsync(string sku, Guid? excludingVariantId = null, CancellationToken cancellationToken = default)
        => await _context.ProductVariants.AnyAsync(
            v => v.Sku == sku && (!excludingVariantId.HasValue || v.Id != excludingVariantId.Value),
            cancellationToken);

    public async Task<bool> ExistsBySignatureAsync(Guid productId, string signature, Guid? excludingVariantId = null, CancellationToken cancellationToken = default)
        => await _context.ProductVariants.AnyAsync(
            v => v.ProductId == productId
              && v.VariantSignature == signature
              && (!excludingVariantId.HasValue || v.Id != excludingVariantId.Value),
            cancellationToken);

    public async Task<ProductVariant?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
        => await _context.ProductVariants
            .Include(v => v.AttributeValues).ThenInclude(av => av.Attribute)
            .Include(v => v.AttributeValues).ThenInclude(av => av.AttributeValue)
            .FirstOrDefaultAsync(v => v.Sku == sku, cancellationToken);

    public async Task<VariantDetailsResponse?> GetDetailsByIdAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        return await _context.ProductVariants
            .AsNoTracking()
            .Where(v => v.Id == variantId)
            .Select(v => new VariantDetailsResponse
            {
                Id = v.Id,
                ProductId = v.ProductId,
                Sku = v.Sku,
                PriceAmount = v.PriceAmount,
                Currency = v.Currency,
                Status = v.Status,
                VariantSignature = v.VariantSignature,
                CreatedAt = v.CreatedAt,
                RowVersion = EF.Property<uint>(v, "xmin"),
                Attributes = v.AttributeValues.Select(av => new VariantAttributeDto
                {
                    AttributeId = av.AttributeId,
                    AttributeName = av.Attribute.Name,
                    AttributeValueId = av.AttributeValueId,
                    AttributeValue = av.AttributeValue.Value
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<VariantDetailsResponse?> GetDetailsBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await _context.ProductVariants
            .AsNoTracking()
            .Where(v => v.Sku == sku)
            .Select(v => new VariantDetailsResponse
            {
                Id = v.Id,
                ProductId = v.ProductId,
                Sku = v.Sku,
                PriceAmount = v.PriceAmount,
                Currency = v.Currency,
                Status = v.Status,
                VariantSignature = v.VariantSignature,
                CreatedAt = v.CreatedAt,
                RowVersion = EF.Property<uint>(v, "xmin"),
                Attributes = v.AttributeValues.Select(av => new VariantAttributeDto
                {
                    AttributeId = av.AttributeId,
                    AttributeName = av.Attribute.Name,
                    AttributeValueId = av.AttributeValueId,
                    AttributeValue = av.AttributeValue.Value
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<VariantListItemResponse>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
        => await _context.ProductVariants
            .AsNoTracking()
            .Where(v => v.ProductId == productId)
            .OrderBy(v => v.Sku)
            .Select(v => new VariantListItemResponse
            {
                Id = v.Id,
                Sku = v.Sku,
                PriceAmount = v.PriceAmount,
                Currency = v.Currency,
                Status = v.Status
            })
            .ToListAsync(cancellationToken);
}
