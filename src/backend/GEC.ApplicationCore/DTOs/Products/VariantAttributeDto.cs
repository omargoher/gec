namespace GEC.ApplicationCore.DTOs.Products;

public class VariantAttributeDto
{
    public Guid AttributeId { get; set; }
    public string AttributeName { get; set; } = string.Empty;
    public Guid AttributeValueId { get; set; }
    public string AttributeValue { get; set; } = string.Empty;
}
