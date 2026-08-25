using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GEC.ApplicationCore.DTOs.Carts;
using GEC.ApplicationCore.Interfaces.Services;
using GEC.API.Services;

namespace GEC.API.Controllers;

[ApiController]
[Route("api/cart")]
[Produces("application/json")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly ICartResolver _cartResolver;

    public CartController(ICartService cartService, ICartResolver cartResolver)
    {
        _cartService = cartService;
        _cartResolver = cartResolver;
    }

    [HttpGet]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CartResponse>> GetCart(CancellationToken cancellationToken)
    {
        var cartId = await _cartResolver.ResolveAsync(HttpContext, cancellationToken);
        var response = await _cartService.GetCartResponseAsync(cartId, cancellationToken);
        return Ok(response);
    }

    [HttpPost("items")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartResponse>> AddItem(
        [FromBody] AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var cartId = await _cartResolver.ResolveAsync(HttpContext, cancellationToken);
        var response = await _cartService.AddItemAsync(cartId, request, cancellationToken);
        return Ok(response);
    }

    [HttpPut("items/{cartItemId:guid}")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartResponse>> UpdateItemQuantity(
        Guid cartItemId,
        [FromBody] UpdateCartItemQuantityRequest request,
        CancellationToken cancellationToken)
    {
        var cartId = await _cartResolver.ResolveAsync(HttpContext, cancellationToken);
        var response = await _cartService.UpdateItemQuantityAsync(cartId, cartItemId, request, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("items/{cartItemId:guid}")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartResponse>> RemoveItem(
        Guid cartItemId,
        CancellationToken cancellationToken)
    {
        var cartId = await _cartResolver.ResolveAsync(HttpContext, cancellationToken);
        var response = await _cartService.RemoveItemAsync(cartId, cartItemId, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("items")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CartResponse>> ClearCart(CancellationToken cancellationToken)
    {
        var cartId = await _cartResolver.ResolveAsync(HttpContext, cancellationToken);
        var response = await _cartService.ClearCartAsync(cancellationToken: cancellationToken, cartId: cartId);
        return Ok(response);
    }
}