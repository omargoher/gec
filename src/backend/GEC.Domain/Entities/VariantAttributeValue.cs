using System;

namespace GEC.Domain.Entities;

public class VariantAttributeValue
{
    public Guid VariantId { get; set; }
    public Guid AttributeId { get; set; }
    public Guid AttributeValueId { get; set; }
    public Guid ProductId { get; set; }

    public ProductVariant Variant { get; set; } = null!;
    public AttributeDefinition Attribute { get; set; } = null!;
    public AttributeValue AttributeValue { get; set; } = null!;
}
