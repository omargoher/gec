using GEC.ApplicationCore.Interfaces.Repositories;
using GEC.Domain.Entities;
using GEC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GEC.Infrastructure.Repositories;

public sealed class WishlistRepository
    : BaseRepository<Wishlist>, IWishlistRepository
{
    private readonly ApplicationDbContext _context;

    public WishlistRepository(ApplicationDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<Wishlist?> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Wishlists
            .FirstOrDefaultAsync(
                x => x.CustomerId == customerId,
                cancellationToken);
    }
}
