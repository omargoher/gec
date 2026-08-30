using GEC.ApplicationCore.DTOs.Favourites;
using GEC.ApplicationCore.Exceptions;
using GEC.ApplicationCore.Interfaces.Persistence;
using GEC.ApplicationCore.Interfaces.Repositories;
using GEC.ApplicationCore.Interfaces.Services;
using GEC.Domain.Entities;
using GEC.Domain.Enums;

namespace GEC.ApplicationCore.Services;

public sealed class FavouriteService : IFavouriteService
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IWishlistItemRepository _wishlistItemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FavouriteService(
        IWishlistRepository wishlistRepository,
        IWishlistItemRepository wishlistItemRepository,
        IUnitOfWork unitOfWork)
    {
        _wishlistRepository = wishlistRepository;
        _wishlistItemRepository = wishlistItemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task AddAsync(
        Guid customerId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        // 1. Validate product existence and status from the local Products table.
        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);

        if (product is null)
            throw new NotFoundException("Product", productId);

        if (product.Status != ProductStatus.Active)
            throw new InvalidRequestException("Only active products can be added to favourites.");

        // 2. Lazy wishlist creation.
        var wishlist = await _wishlistRepository.GetByCustomerIdAsync(
            customerId,
            cancellationToken);

        if (wishlist is null)
        {
            wishlist = new Wishlist
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId
            };

            _wishlistRepository.Add(wishlist);
        }

        // 3. Application-level duplicate check (friendly conflict response).
        var existingItem = await _wishlistItemRepository
            .GetByWishlistIdAndProductIdAsync(
                wishlist.Id,
                productId,
                cancellationToken);

        if (existingItem is not null)
            throw new ConflictException("Product is already in favourites");

        // 4. Create the wishlist item.
        var item = new WishlistItem
        {
            Id = Guid.NewGuid(),
            WishlistId = wishlist.Id,
            ProductId = productId,
            AddedAt = DateTime.UtcNow
        };

        _wishlistItemRepository.Add(item);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(
        Guid customerId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var wishlist = await _wishlistRepository.GetByCustomerIdAsync(
            customerId,
            cancellationToken);

        if (wishlist is null)
            return; // Idempotent — no wishlist means nothing to remove.

        var item = await _wishlistItemRepository
            .GetByWishlistIdAndProductIdAsync(
                wishlist.Id,
                productId,
                cancellationToken);

        if (item is null)
            return; // Idempotent — already not a favourite.

        _wishlistItemRepository.Remove(item);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<WishlistResponse> GetAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var wishlist = await _wishlistRepository.GetByCustomerIdAsync(
            customerId,
            cancellationToken);

        if (wishlist is null)
            return new WishlistResponse(new List<FavouriteItemResponse>());

        // Single query: left join WishlistItems → Products in the repository.
        var items = await _wishlistItemRepository.GetFavouriteItemsAsync(
            wishlist.Id,
            cancellationToken);

        return new WishlistResponse(items);
    }

    public async Task<bool> IsFavouriteAsync(
        Guid customerId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var wishlist = await _wishlistRepository.GetByCustomerIdAsync(
            customerId,
            cancellationToken);

        if (wishlist is null)
            return false;

        return await _wishlistItemRepository.ExistsAsync(
            wishlist.Id,
            productId,
            cancellationToken);
    }
}
