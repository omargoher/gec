using GEC.ApplicationCore.DTOs;
using GEC.ApplicationCore.DTOs.Products;
using GEC.ApplicationCore.Interfaces.Repositories;
using GEC.Domain.Entities;
using GEC.Domain.Enums;
using GEC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using GEC.Infrastructure.Extensions;

namespace GEC.Infrastructure.Repositories;

public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default)
        => await _context.Products.AnyAsync(p => p.Slug == slug, cancellationToken);

    public async Task<bool> HasActiveVariantsAsync(Guid productId, CancellationToken cancellationToken = default)
        => await _context.ProductVariants.AnyAsync(
            v => v.ProductId == productId && v.Status == ProductVariantStatus.Active, cancellationToken);

    public async Task<ProductDetailsResponse?> GetDetailsByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.Id == productId)
            .Select(p => new ProductDetailsResponse
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                BaseDescription = p.BaseDescription,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,

                RowVersion = EF.Property<uint>(p, "xmin"),

                Specifications = p.Specifications
                    .Select(s => new ProductSpecificationResponse
                    {
                        Id = s.Id,
                        ProductId = s.ProductId,
                        Name = s.Name,
                        Value = s.Value
                    })
                    .ToList(),

                Variants = p.Variants
                    .Select(v => new VariantListItemResponse
                    {
                        Id = v.Id,
                        Sku = v.Sku,
                        PriceAmount = v.PriceAmount,
                        Currency = v.Currency,
                        Status = v.Status
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
    //Get a page of products from the database,
    //optionally filter them, count how many match,
    //then return only the products needed for the current page.
    public async Task<PagedResult<ProductListItemResponse>> GetPagedAsync(ProductFilterParams filterParams, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .Search(filterParams.SearchTerm)
            .FilterByStatus(filterParams.Status)
            .FilterByCategory(filterParams.categoryId)
            .ApplySort(filterParams.SortBy, filterParams.SortOrder)
            .Select(p => new ProductListItemResponse
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                Status = p.Status
            })
            .ToPagedListAsync(filterParams.Pagination.PageNumber, filterParams.Pagination.PageSize, cancellationToken);
    }
}
