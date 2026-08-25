using GEC.ApplicationCore.Exceptions;
using GEC.ApplicationCore.Interfaces.Identity;
using GEC.ApplicationCore.Interfaces.Persistence;
using GEC.Domain.Entities;

namespace GEC.API.Services;

public class CartResolver : ICartResolver
{    
    private const string CookieName = "cart_id";
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    
    public CartResolver(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }
    
    public async Task<Guid> ResolveAsync(HttpContext httpContext, CancellationToken cancellationToken = default)
    {
        var customerId = await _currentUserService.GetCustomerIdAsync(cancellationToken);
        var cookieCartId = GetCartIdFromCookie(httpContext);

        var cart = await TryGetFromCookieAsync(cookieCartId, customerId, cancellationToken)
                   ?? await TryGetByCustomerIdAsync(customerId, cancellationToken)
                   ?? await CreateNewCartAsync(customerId, cancellationToken);

        if (cookieCartId != cart.Id)
            SetCartCookie(httpContext, cart.Id);

        return cart.Id;
    }
    
    private async Task<Cart?> TryGetFromCookieAsync(Guid? cookieCartId, Guid? customerId, CancellationToken cancellationToken = default)
    {
        if (cookieCartId is null) return null;

        var cart = await _unitOfWork.Carts.GetByIdWithItemsAsync(cookieCartId.Value, cancellationToken);
        if (cart is null) return null;

        return IsOwnedByCurrentUser(cart, customerId) ? cart : null;
    }

    private static bool IsOwnedByCurrentUser(Cart cart, Guid? customerId)
    {
        if (cart.CustomerId is null)
        {
            return customerId is null;
        }
        else
        {
            return cart.CustomerId == customerId;
        }
    }
    
    private async Task<Cart?> TryGetByCustomerIdAsync(Guid? customerId, CancellationToken cancellationToken = default)
    {
        if (customerId is null) return null;
        return await _unitOfWork.Carts.GetByCustomerIdWithItemsAsync(customerId.Value, cancellationToken);
    }
    
    private async Task<Cart> CreateNewCartAsync(Guid? customerId, CancellationToken cancellationToken = default)
    {
        var cart = customerId is not null
            ? Cart.CreateForCustomer(customerId.Value)
            : Cart.CreateForAnonymous();

        _unitOfWork.Carts.Add(cart);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return cart;
    }
    
    private static Guid? GetCartIdFromCookie(HttpContext context)
    {
        var cartCookie = context.Request.Cookies[CookieName];

        if (Guid.TryParse(cartCookie, out var id)) return id;

        return null;
    }
    
    private static void SetCartCookie(HttpContext context, Guid cartId)
    {
        context.Response.Cookies.Append(CookieName, cartId.ToString(), new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });
    }
}