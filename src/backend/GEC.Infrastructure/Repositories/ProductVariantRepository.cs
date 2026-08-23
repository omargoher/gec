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
        var variant = await _context.ProductVariants
            .Include(v => v.AttributeValues).ThenInclude(av => av.Attribute)
            .Include(v => v.AttributeValues).ThenInclude(av => av.AttributeValue)
            .FirstOrDefaultAsync(v => v.Id == variantId, cancellationToken);

        if (variant is null) return null;

        var rowVersion = _context.Entry(variant).Property<uint>("xmin").CurrentValue;

        return MapToDetails(variant, rowVersion);
    }

    public async Task<VariantDetailsResponse?> GetDetailsBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        var variant = await _context.ProductVariants
            .Include(v => v.AttributeValues).ThenInclude(av => av.Attribute)
            .Include(v => v.AttributeValues).ThenInclude(av => av.AttributeValue)
            .FirstOrDefaultAsync(v => v.Sku == sku, cancellationToken);

        if (variant is null) return null;

        var rowVersion = _context.Entry(variant).Property<uint>("xmin").CurrentValue;

        return MapToDetails(variant, rowVersion);
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

    public static VariantDetailsResponse MapToDetails(ProductVariant variant, uint rowVersion) =>
        new()
        {
            Id = variant.Id,
            ProductId = variant.ProductId,
            Sku = variant.Sku,
            PriceAmount = variant.PriceAmount,
            Currency = variant.Currency,
            Status = variant.Status,
            VariantSignature = variant.VariantSignature,
            CreatedAt = variant.CreatedAt,
            RowVersion = rowVersion,
            Attributes = variant.AttributeValues.Select(av => new VariantAttributeDto
            {
                AttributeId = av.AttributeId,
                AttributeName = av.Attribute?.Name ?? string.Empty,
                AttributeValueId = av.AttributeValueId,
                AttributeValue = av.AttributeValue?.Value ?? string.Empty
            }).ToList()
        };
}
