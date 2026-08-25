using GEC.Domain.Enums;
namespace GEC.ApplicationCore.DTOs.Products;

public class ProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string BaseDescription { get; set; } = string.Empty;
    public ProductStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public uint RowVersion { get; set; }
}
