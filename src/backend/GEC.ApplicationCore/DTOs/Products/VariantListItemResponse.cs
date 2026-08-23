using GEC.Domain.Enums;
namespace GEC.ApplicationCore.DTOs.Products;

public class VariantListItemResponse
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal PriceAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public ProductVariantStatus Status { get; set; }
}
