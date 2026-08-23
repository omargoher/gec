namespace GEC.ApplicationCore.DTOs.Attributes;

public class AttributeValueResponse
{
    public Guid Id { get; set; }
    public Guid AttributeId { get; set; }
    public string Value { get; set; } = string.Empty;
}
