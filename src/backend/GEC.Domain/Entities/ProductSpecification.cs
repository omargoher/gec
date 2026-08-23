using System;

namespace GEC.Domain.Entities;

public class ProductSpecification : BaseEntity
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public Product Product { get; set; } = null!;
}
