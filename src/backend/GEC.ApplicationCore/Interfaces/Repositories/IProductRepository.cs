using GEC.ApplicationCore.DTOs;
using GEC.ApplicationCore.DTOs.Products;
using GEC.Domain.Entities;

namespace GEC.ApplicationCore.Interfaces.Repositories;

public interface IProductRepository : IBaseRepository<Product>
{
    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> HasActiveVariantsAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<ProductDetailsResponse?> GetDetailsByIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductListItemResponse>> GetPagedAsync(GetProductsRequest request, CancellationToken cancellationToken = default);
}
