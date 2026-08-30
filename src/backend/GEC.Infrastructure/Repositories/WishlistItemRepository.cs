using GEC.ApplicationCore.DTOs.Favourites;
using GEC.ApplicationCore.Interfaces.Repositories;
using GEC.Domain.Entities;
using GEC.Domain.Enums;
using GEC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GEC.Infrastructure.Repositories;

public sealed class WishlistItemRepository
    : BaseRepository<WishlistItem>, IWishlistItemRepository
{
    private readonly ApplicationDbContext _context;

    public WishlistItemRepository(ApplicationDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<FavouriteItemResponse>> GetFavouriteItemsAsync(
        Guid wishlistId,
        CancellationToken cancellationToken = default)
    {
        return await (
            from wi in _context.WishlistItems.AsNoTracking()
            where wi.WishlistId == wishlistId
            join p in _context.Products.AsNoTracking()
                on wi.ProductId equals p.Id into products
            from p in products.DefaultIfEmpty()
            select new FavouriteItemResponse(
                wi.ProductId,
                p != null ? p.Name : null,
                p != null ? p.Slug : null,
                null,                                          // No ImageUrl on Product entity
                p != null && p.Status == ProductStatus.Active,
                wi.AddedAt)
        ).ToListAsync(cancellationToken);
    }

    public async Task<WishlistItem?> GetByWishlistIdAndProductIdAsync(
        Guid wishlistId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await _context.WishlistItems
            .FirstOrDefaultAsync(
                x => x.WishlistId == wishlistId &&
                     x.ProductId == productId,
                cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid wishlistId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await _context.WishlistItems
            .AnyAsync(
                x => x.WishlistId == wishlistId &&
                     x.ProductId == productId,
                cancellationToken);
    }
}
