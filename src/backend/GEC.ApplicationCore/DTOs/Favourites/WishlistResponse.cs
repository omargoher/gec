namespace GEC.ApplicationCore.DTOs.Favourites;

/// <summary>
/// The customer's complete wishlist, containing all favourited products.
/// Returns an empty Items collection when the customer has no wishlist yet.
/// </summary>
public sealed record WishlistResponse(
    IReadOnlyList<FavouriteItemResponse> Items);
