using GEC.Domain.Enums;
namespace GEC.ApplicationCore.DTOs.Products;

public class ChangeVariantStatusRequest
{
    public ProductVariantStatus Status { get; set; }
    public uint RowVersion { get; set; }
}
