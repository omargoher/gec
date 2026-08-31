using System;
using System.Threading;
using System.Threading.Tasks;
using GEC.ApplicationCore.DTOs.Carts;
using GEC.Domain.Entities;

namespace GEC.ApplicationCore.Interfaces.Services;

public interface ICartService
{
    Task<CartResponse> GetCartResponseAsync(Guid cartId, CancellationToken cancellationToken = default);
    Task<CartResponse> AddItemAsync(Guid cartId, AddCartItemRequest request, CancellationToken cancellationToken = default);
    Task<CartResponse> UpdateItemQuantityAsync(Guid cartId, Guid cartItemId, UpdateCartItemQuantityRequest request, CancellationToken cancellationToken = default);
    Task<CartResponse> RemoveItemAsync(Guid cartId, Guid cartItemId, CancellationToken cancellationToken = default);
    Task<CartResponse> ClearCartAsync(Guid cartId, CancellationToken cancellationToken = default);

    Task<Guid> MergeGuestCartIntoCustomerAsync(Guid guestCartId, Guid customerId,
        CancellationToken cancellationToken = default);
}
