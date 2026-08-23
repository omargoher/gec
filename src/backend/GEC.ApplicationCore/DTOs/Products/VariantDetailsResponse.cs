using GEC.Domain.Enums;
namespace GEC.ApplicationCore.DTOs.Products;

public class VariantDetailsResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal PriceAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public ProductVariantStatus Status { get; set; }
    public string VariantSignature { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<VariantAttributeDto> Attributes { get; set; } = new();
    public uint RowVersion { get; set; }
}
