namespace GEC.ApplicationCore.DTOs.Favourites;

/// <summary>
/// Request body for adding a product to favourites.
/// CustomerId is resolved server-side from the authenticated JWT.
/// </summary>
public sealed record AddFavouriteRequest(
    Guid ProductId);
