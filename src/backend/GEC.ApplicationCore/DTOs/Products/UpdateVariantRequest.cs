namespace GEC.ApplicationCore.DTOs.Products;

public class UpdateVariantRequest
{
    public string? Sku { get; set; }
    public decimal? PriceAmount { get; set; }
    public string? Currency { get; set; }
    public uint RowVersion { get; set; }
}
