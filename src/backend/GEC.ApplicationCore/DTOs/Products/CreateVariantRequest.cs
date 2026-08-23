namespace GEC.ApplicationCore.DTOs.Products;

public class CreateVariantRequest
{
    public string Sku { get; set; } = string.Empty;
    public decimal PriceAmount { get; set; }
    public string Currency { get; set; } = "EGP";
}
