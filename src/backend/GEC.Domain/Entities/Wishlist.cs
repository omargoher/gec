namespace GEC.Domain.Entities;

public class Wishlist
{
    public Guid Id { get; set; }

    /// <summary>
    /// Plain scalar reference to the customer — no local FK.
    /// Customer is owned by the Identity/Customer service.
    /// Value is resolved from the authenticated JWT.
    /// </summary>
    public Guid CustomerId { get; set; }

    public ICollection<WishlistItem> Items { get; set; }
        = new List<WishlistItem>();
}
