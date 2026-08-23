using GEC.ApplicationCore.DTOs;
using GEC.Domain.Enums;
namespace GEC.ApplicationCore.DTOs.Products;

public class GetProductsRequest : PaginationParams
{
    public string? Search { get; set; }
    public ProductStatus? Status { get; set; }
}
