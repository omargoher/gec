using GEC.ApplicationCore.DTOs.Categories;
using GEC.ApplicationCore.Exceptions;
using GEC.ApplicationCore.Interfaces.Repositories;
using GEC.Domain.Entities;
using GEC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GEC.Infrastructure.Repositories;

public class ProductCategoryRepository : BaseRepository<ProductCategory>, IProductCategoryRepository
{
    private readonly ApplicationDbContext _context;

    public ProductCategoryRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(Guid productId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.ProductsCategories
            .AnyAsync(
                pc => pc.CategoryId == categoryId && pc.ProductId == productId, cancellationToken);
    }

    public async Task<ProductCategory?> GetAsync(Guid productId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.ProductsCategories
            .FirstOrDefaultAsync(
                pc => pc.CategoryId == categoryId && pc.ProductId == productId, cancellationToken);
    }
}