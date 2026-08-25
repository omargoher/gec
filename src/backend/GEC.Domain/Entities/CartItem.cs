using System;

namespace GEC.Domain.Entities;

public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    public Guid VariantId { get; set; }
    public int Quantity { get; set; }

    public Cart Cart { get; set; } = null!;
    
    private CartItem() { }
    
    public static CartItem Create(Guid cartId, Guid variantId, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");

        return new CartItem
        {
            CartId = cartId,
            VariantId = variantId,
            Quantity = quantity,
        };
    }

    public void SetQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive. Remove the item instead of setting quantity to zero.");

        Quantity = quantity;
    }
}
