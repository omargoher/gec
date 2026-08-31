using System;
using System.Collections.Generic;

namespace GEC.Domain.Entities;

public class Cart : BaseEntity
{
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public DateTime LastActive { get; set; }
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    public uint RowVersion { get; set; }
    
    private Cart() { }
    
    public static Cart CreateForCustomer(Guid customerId)
    {
        return new Cart
        {
            CustomerId = customerId,
            LastActive = DateTime.UtcNow
        };
    }

    public static Cart CreateForAnonymous()
    {
        return new Cart
        {
            LastActive = DateTime.UtcNow
        };
    }
    
    
    public void AttachToCustomer(Guid customerId)
    {
        if (CustomerId is not null)
            throw new InvalidOperationException("Cart is already attached to a customer.");

        CustomerId = customerId;
        LastActive = DateTime.UtcNow;
    }

    public void AddOrUpdateItem(Guid variantId, int quantity)
    {
        var existing = Items.FirstOrDefault(i => i.VariantId == variantId);
        if (existing is not null)
        {
            existing.SetQuantity(existing.Quantity + quantity);
        }
        else
        {
            Items.Add(CartItem.Create(Id, variantId, quantity));
        }

        LastActive = DateTime.UtcNow;
    }

    public void MergeWith(Cart guestCart)
    {
        foreach (var guestItem in guestCart.Items)
        {
            var existing = Items.FirstOrDefault(i => i.VariantId == guestItem.VariantId);
            if (existing is not null)
                existing.SetQuantity(Math.Max(existing.Quantity, guestItem.Quantity));
            else
                Items.Add(CartItem.Create(Id, guestItem.VariantId, guestItem.Quantity));
        }

        LastActive = DateTime.UtcNow;
    }
}
