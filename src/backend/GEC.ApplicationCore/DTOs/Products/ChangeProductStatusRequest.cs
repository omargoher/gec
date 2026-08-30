using GEC.Domain.Enums;
namespace GEC.ApplicationCore.DTOs.Products;

public class ChangeProductStatusRequest
{
    public ProductStatus Status { get; set; }
    public uint RowVersion { get; set; }
}
