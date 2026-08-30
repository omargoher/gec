namespace GEC.Domain.Entities;

public class WishlistItem
{
    public Guid Id { get; set; }

    public Guid WishlistId { get; set; }
    public Wishlist Wishlist { get; set; } = null!;

    /// <summary>
    /// Plain scalar reference to the product — no local FK.
    /// Product is owned by the Catalog service.
    /// Display data is fetched on demand via ICatalogProductClient.
    /// </summary>
    public Guid ProductId { get; set; }

    public DateTime AddedAt { get; set; }
}
