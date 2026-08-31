using GEC.ApplicationCore.DTOs.Favourites;
using GEC.Domain.Entities;

namespace GEC.ApplicationCore.Interfaces.Repositories;

/// <summary>
/// Persistence contract for WishlistItem entities.
/// Custom methods extend IBaseRepository&lt;WishlistItem&gt;.
/// </summary>
public interface IWishlistItemRepository : IBaseRepository<WishlistItem>
{
    /// <summary>
    /// Returns all favourited items in a wishlist, with product display data
    /// joined directly from the Products table via a left join.
    /// Items whose product no longer exists return null display fields
    /// and IsAvailable = false.
    /// </summary>
    Task<IReadOnlyList<FavouriteItemResponse>> GetFavouriteItemsAsync(
        Guid wishlistId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the specific wishlist item for a given product,
    /// or null when the product has not been favourited.
    /// </summary>
    Task<WishlistItem?> GetByWishlistIdAndProductIdAsync(
        Guid wishlistId,
        Guid productId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Efficient existence check — uses AnyAsync rather than loading the full entity.
    /// </summary>
    Task<bool> ExistsAsync(
        Guid wishlistId,
        Guid productId,
        CancellationToken cancellationToken = default);
}
