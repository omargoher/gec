using System;

namespace GEC.Domain.Entities;

public class ProductAttribute
{
    public Guid ProductId { get; set; }
    public Guid AttributeId { get; set; }
    public bool IsRequired { get; set; } = true;

    public Product Product { get; set; } = null!;
    public AttributeDefinition Attribute { get; set; } = null!;
}
