using GEC.ApplicationCore.DTOs.Favourites;

namespace GEC.ApplicationCore.Interfaces.Services;

/// <summary>
/// Business operations for the customer's product wishlist (favourites).
/// All methods identify the customer by a Guid resolved from the JWT —
/// the customerId is never supplied by the client directly.
/// </summary>
public interface IFavouriteService
{
    /// <summary>
    /// Adds a product to the customer's favourites.
    /// Validates product existence and Active status from the local Products table.
    /// Creates the customer's wishlist lazily on first add.
    /// </summary>
    /// <exception cref="Exceptions.NotFoundException">Product does not exist.</exception>
    /// <exception cref="Exceptions.InvalidRequestException">Product is not Active.</exception>
    /// <exception cref="Exceptions.ConflictException">Product already in favourites.</exception>
    Task AddAsync(
        Guid customerId,
        Guid productId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a product from the customer's favourites.
    /// Idempotent — returns normally when the item or wishlist does not exist.
    /// </summary>
    Task RemoveAsync(
        Guid customerId,
        Guid productId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the customer's full wishlist with product display data
    /// joined directly from the Products table.
    /// Returns an empty list when no wishlist exists.
    /// </summary>
    Task<WishlistResponse> GetAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a product is in the customer's favourites.
    /// Returns false when the customer has no wishlist.
    /// </summary>
    Task<bool> IsFavouriteAsync(
        Guid customerId,
        Guid productId,
        CancellationToken cancellationToken = default);
}
