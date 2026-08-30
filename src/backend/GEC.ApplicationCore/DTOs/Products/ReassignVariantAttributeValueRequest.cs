namespace GEC.ApplicationCore.DTOs.Products;

public class ReassignVariantAttributeValueRequest
{
    public Guid AttributeId { get; set; }
    public Guid NewAttributeValueId { get; set; }
    public uint RowVersion { get; set; }
}
