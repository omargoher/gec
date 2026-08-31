using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GEC.ApplicationCore.DTOs.Carts;
using GEC.ApplicationCore.Exceptions;
using GEC.ApplicationCore.Interfaces.Persistence;
using GEC.ApplicationCore.Interfaces.Services;
using GEC.Domain.Entities;
using GEC.Domain.Enums;

namespace GEC.ApplicationCore.Services;

public class CartService : ICartService
{
    private readonly IUnitOfWork _unitOfWork;

    public CartService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CartResponse> GetCartResponseAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        var cart = await _unitOfWork.Carts.GetByIdWithItemsAsync(cartId, cancellationToken)
                   ?? throw new NotFoundException("Cart", cartId);
        return await MapToResponseAsync(cart, cancellationToken);
    }

    public async Task<CartResponse> AddItemAsync(Guid cartId, AddCartItemRequest request, CancellationToken cancellationToken = default)
    {
        var cart = await _unitOfWork.Carts.GetByIdWithItemsAsync(cartId, cancellationToken)
                   ?? throw new NotFoundException("Cart", cartId);

        var variantInfo = await GetPurchasableVariantAsync(request.VariantId, cancellationToken);
        if (variantInfo is null)
            throw new NotFoundException("Variant", request.VariantId);
        if (!variantInfo.IsPurchasable)
            throw new InvalidRequestException("Variant is not purchasable.");

        cart.AddOrUpdateItem(request.VariantId, request.Quantity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await MapToResponseAsync(cart, cancellationToken);
    }

    public async Task<CartResponse> UpdateItemQuantityAsync(Guid cartId, Guid cartItemId, UpdateCartItemQuantityRequest request, CancellationToken cancellationToken = default)
    {
        var cart = await _unitOfWork.Carts.GetByIdWithItemsAsync(cartId, cancellationToken)
               ?? throw new NotFoundException("Cart", cartId);
        
        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
        if (item is null)
            throw new NotFoundException("CartItem", cartItemId);

        item.SetQuantity(request.Quantity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await MapToResponseAsync(cart, cancellationToken);
    }

    public async Task<CartResponse> RemoveItemAsync(Guid cartId, Guid cartItemId, CancellationToken cancellationToken = default)
    {
        var cart = await _unitOfWork.Carts.GetByIdWithItemsAsync(cartId, cancellationToken)
               ?? throw new NotFoundException("Cart", cartId);
        
        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
        if (item is null)
            throw new NotFoundException("CartItem", cartItemId);

        cart.Items.Remove(item);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await MapToResponseAsync(cart, cancellationToken);
    }

    public async Task<CartResponse> ClearCartAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        var cart = await _unitOfWork.Carts.GetByIdWithItemsAsync(cartId, cancellationToken)
               ?? throw new NotFoundException("Cart", cartId);
        
        cart.Items.Clear();
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await MapToResponseAsync(cart, cancellationToken);
    }

    public async Task<Guid> MergeGuestCartIntoCustomerAsync(Guid guestCartId, Guid customerId, CancellationToken cancellationToken = default)
    {
        var guestCart = await _unitOfWork.Carts.GetByIdWithItemsAsync(guestCartId, cancellationToken);
        var customerCart = await _unitOfWork.Carts.GetByCustomerIdWithItemsAsync(customerId, cancellationToken);
        
        Guid resultCartId;
        
        if (guestCart is null)
            return customerCart.Id;

        if (guestCart.CustomerId is not null && guestCart.CustomerId != customerId)
            return customerCart.Id;

        if (guestCart.CustomerId == customerId)
            return customerCart.Id;


        if (customerCart is null)
        {
            guestCart.AttachToCustomer(customerId);
            resultCartId = guestCart.Id;
        }
        else
        {
            customerCart.MergeWith(guestCart);
            _unitOfWork.Carts.Remove(guestCart);
            resultCartId = customerCart.Id;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return resultCartId;
    }
    
    private async Task<Cart> GetCartOrThrowAsync(Guid cartId, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Carts.GetByIdWithItemsAsync(cartId, cancellationToken)
               ?? throw new NotFoundException("Cart", cartId);
    }
    
    private async Task<CartVariantInfo?> GetPurchasableVariantAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        var variant = await _unitOfWork.ProductVariants.GetDetailsByIdAsync(variantId, cancellationToken);
        if (variant is null) return null;

        var product = await _unitOfWork.Products.GetDetailsByIdAsync(variant.ProductId, cancellationToken);
        if (product is null) return null;

        var isPurchasable = variant.Status == ProductVariantStatus.Active && product.Status == ProductStatus.Active;

        return new CartVariantInfo(
            Id: variant.Id,
            PriceAmount: variant.PriceAmount,
            Currency: variant.Currency,
            IsPurchasable: isPurchasable
        );
    }
    
    private async Task<CartResponse> MapToResponseAsync(Cart cart, CancellationToken cancellationToken)
    {
        decimal total = 0;
        string currency = "EGP";
        var itemResponses = new List<CartItemResponse>();

        if (!cart.Items.Any())
            return new CartResponse(cart.Id, itemResponses, total, currency);

        var variantTasks = cart.Items.Select(async item =>
        {
            var variantInfo = await GetPurchasableVariantAsync(item.VariantId, cancellationToken);
            return (Item: item, VariantInfo: variantInfo);
        });

        var results = await Task.WhenAll(variantTasks);

        foreach (var (item, variantInfo) in results)
        {
            var isAvailable = variantInfo is not null && variantInfo.IsPurchasable;
            var unitPrice = variantInfo?.PriceAmount ?? 0;
            var subtotal = unitPrice * item.Quantity;

            if (isAvailable)
            {
                total += subtotal;
                currency = variantInfo!.Currency;
            }

            itemResponses.Add(new CartItemResponse(
                Id: item.Id,
                VariantId: item.VariantId,
                Quantity: item.Quantity,
                UnitPrice: unitPrice,
                Subtotal: subtotal,
                Currency: variantInfo?.Currency ?? currency,
                IsAvailable: isAvailable
            ));
        }

        return new CartResponse(cart.Id, itemResponses, total, currency);
    }
}