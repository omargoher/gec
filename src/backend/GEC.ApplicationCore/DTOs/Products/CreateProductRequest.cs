namespace GEC.ApplicationCore.DTOs.Products;

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string BaseDescription { get; set; } = string.Empty;
}
