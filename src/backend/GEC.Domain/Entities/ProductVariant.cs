using System;
using System.Collections.Generic;
using GEC.Domain.Enums;

namespace GEC.Domain.Entities;

public class ProductVariant : BaseEntity
{
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal PriceAmount { get; set; }
    public string Currency { get; set; } = "EGP";
    public ProductVariantStatus Status { get; set; } = ProductVariantStatus.Draft;
    public string VariantSignature { get; set; } = string.Empty;

    public Product Product { get; set; } = null!;
    public ICollection<VariantAttributeValue> AttributeValues { get; set; } = new List<VariantAttributeValue>();
}
