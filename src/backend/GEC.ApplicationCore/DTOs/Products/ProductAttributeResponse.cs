namespace GEC.ApplicationCore.DTOs.Products;

public class ProductAttributeResponse
{
    public Guid ProductId { get; set; }
    public Guid AttributeId { get; set; }
    public string AttributeName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
}
