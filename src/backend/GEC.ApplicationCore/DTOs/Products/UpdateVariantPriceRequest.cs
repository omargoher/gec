namespace GEC.ApplicationCore.DTOs.Products;

public class UpdateVariantPriceRequest
{
    public decimal PriceAmount { get; set; }
    public string Currency { get; set; } = "EGP";
    public uint RowVersion { get; set; }
}
