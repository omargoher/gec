namespace GEC.ApplicationCore.DTOs.Products;

public class ProductSpecificationResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
