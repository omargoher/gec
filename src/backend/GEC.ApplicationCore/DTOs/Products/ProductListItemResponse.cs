using GEC.Domain.Enums;
namespace GEC.ApplicationCore.DTOs.Products;

public class ProductListItemResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public ProductStatus Status { get; set; }
}
