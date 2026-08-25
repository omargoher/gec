using System;
using System.Collections.Generic;

namespace GEC.Domain.Entities;

public class AttributeValue : BaseEntity
{
    public Guid AttributeId { get; set; }
    public string Value { get; set; } = string.Empty;

    public AttributeDefinition Attribute { get; set; } = null!;
    public ICollection<VariantAttributeValue> VariantAttributeValues { get; set; } = new List<VariantAttributeValue>();
}
