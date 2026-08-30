namespace GEC.ApplicationCore.DTOs.Favourites;

/// <summary>
/// Response for the "check if a product is a favourite" endpoint.
/// </summary>
public sealed record FavouriteStatusResponse(
    bool IsFavourite);
