using System;
using System.Collections.Generic;

namespace GEC.Domain.Entities;

public class AttributeDefinition : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<AttributeValue> Values { get; set; } = new List<AttributeValue>();
    public ICollection<ProductAttribute> ProductAttributes { get; set; } = new List<ProductAttribute>();
}
