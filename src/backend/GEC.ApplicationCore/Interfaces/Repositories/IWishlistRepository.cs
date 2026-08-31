using GEC.Domain.Entities;

namespace GEC.ApplicationCore.Interfaces.Repositories;

/// <summary>
/// Persistence contract for the Wishlist aggregate root.
/// Custom methods extend IBaseRepository&lt;Wishlist&gt;.
/// </summary>
public interface IWishlistRepository : IBaseRepository<Wishlist>
{
    /// <summary>
    /// Returns the wishlist belonging to the given customer,
    /// or null when the customer has not yet favourited anything.
    /// </summary>
    Task<Wishlist?> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);
}
