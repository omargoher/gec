namespace GEC.ApplicationCore.DTOs.Products;

public class AddProductAttributeRequest
{
    public Guid AttributeId { get; set; }
    public bool IsRequired { get; set; } = true;
}
