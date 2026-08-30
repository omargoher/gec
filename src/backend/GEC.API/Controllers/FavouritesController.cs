using GEC.ApplicationCore.DTOs.Favourites;
using GEC.ApplicationCore.Interfaces.Identity;
using GEC.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GEC.API.Controllers;

/// <summary>
/// Customer wishlist (favourites) management.
/// All endpoints operate on the authenticated customer —
/// CustomerId is resolved server-side from the JWT, never from the URL.
/// </summary>
[ApiController]
[Route("api/favourites")]
[Authorize]
[Tags("Favourites")]
public sealed class FavouritesController : ControllerBase
{
    private readonly IFavouriteService _favouriteService;
    private readonly ICurrentUserService _currentUserService;

    public FavouritesController(
        IFavouriteService favouriteService,
        ICurrentUserService currentUserService)
    {
        _favouriteService = favouriteService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Returns the authenticated customer's complete favourites list.
    /// Returns an empty list when the customer has never favourited anything.
    /// Product display fields may be null when the Catalog service is unavailable.
    /// </summary>
    /// <response code="200">Wishlist returned (may be empty).</response>
    [HttpGet]
    [ProducesResponseType(typeof(WishlistResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<WishlistResponse>> Get(
        CancellationToken cancellationToken)
    {
        var customerId = await _currentUserService.GetCustomerIdAsync(cancellationToken);

        var response = await _favouriteService.GetAsync(customerId, cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Adds a product to the authenticated customer's favourites.
    /// The product must exist in Catalog and have Active status.
    /// Creates the customer's wishlist on first use.
    /// </summary>
    /// <response code="204">Product added to favourites.</response>
    /// <response code="400">Product is not Active, or request is invalid.</response>
    /// <response code="404">Product does not exist.</response>
    /// <response code="409">Product is already in favourites.</response>
    /// <response code="503">Catalog service is unavailable.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add(
        [FromBody] AddFavouriteRequest request,
        CancellationToken cancellationToken)
    {
        var customerId = await _currentUserService.GetCustomerIdAsync(cancellationToken);

        await _favouriteService.AddAsync(customerId, request.ProductId, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Removes a product from the authenticated customer's favourites.
    /// Idempotent — returns 204 even when the product was not a favourite.
    /// </summary>
    /// <response code="204">Favourite removed (or was already absent).</response>
    [HttpDelete("{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remove(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var customerId = await _currentUserService.GetCustomerIdAsync(cancellationToken);

        await _favouriteService.RemoveAsync(customerId, productId, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Checks whether a product is in the authenticated customer's favourites.
    /// Returns false when the customer has no wishlist yet.
    /// </summary>
    /// <response code="200">Status returned.</response>
    [HttpGet("{productId:guid}/exists")]
    [ProducesResponseType(typeof(FavouriteStatusResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<FavouriteStatusResponse>> IsFavourite(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var customerId = await _currentUserService.GetCustomerIdAsync(cancellationToken);

        var isFavourite = await _favouriteService.IsFavouriteAsync(
            customerId,
            productId,
            cancellationToken);

        return Ok(new FavouriteStatusResponse(isFavourite));
    }
}
