namespace GEC.ApplicationCore.DTOs.Favourites;

/// <summary>
/// Represents one favourited product in the wishlist response.
/// Product display fields (Name, Slug, ImageUrl) are nullable because they
/// are fetched on demand from Catalog — they will be null if Catalog is
/// unreachable or no longer has the product.
/// </summary>
public sealed record FavouriteItemResponse(
    Guid ProductId,
    string? ProductName,
    string? Slug,
    string? ImageUrl,
    bool IsAvailable,
    DateTime AddedAt);
