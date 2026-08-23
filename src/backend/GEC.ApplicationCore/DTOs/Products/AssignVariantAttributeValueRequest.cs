namespace GEC.ApplicationCore.DTOs.Products;

public class AssignVariantAttributeValueRequest
{
    public Guid AttributeId { get; set; }
    public Guid AttributeValueId { get; set; }
    public uint RowVersion { get; set; }
}
