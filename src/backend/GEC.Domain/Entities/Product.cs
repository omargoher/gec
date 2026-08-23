using System;
using System.Collections.Generic;
using GEC.Domain.Enums;

namespace GEC.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string BaseDescription { get; set; } = string.Empty;
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
    public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductAttribute> ProductAttributes { get; set; } = new List<ProductAttribute>();
    public ICollection<ProductSpecification> Specifications { get; set; } = new List<ProductSpecification>();
}
