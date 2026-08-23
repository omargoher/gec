using GEC.ApplicationCore.DTOs.Products;
using GEC.Domain.Entities;

namespace GEC.ApplicationCore.Interfaces.Repositories;

public interface IProductVariantRepository : IBaseRepository<ProductVariant>
{
    Task<bool> ExistsBySkuAsync(string sku, Guid? excludingVariantId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySignatureAsync(Guid productId, string signature, Guid? excludingVariantId = null, CancellationToken cancellationToken = default);
    Task<ProductVariant?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<VariantDetailsResponse?> GetDetailsByIdAsync(Guid variantId, CancellationToken cancellationToken = default);
    Task<VariantDetailsResponse?> GetDetailsBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<List<VariantListItemResponse>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
}
