using GEC.ApplicationCore.DTOs.Products;
using GEC.Domain.Entities;

namespace GEC.ApplicationCore.Interfaces.Repositories;

/// <summary>
/// Repository for ProductAttribute (composite PK: ProductId + AttributeId).
/// Does not inherit IBaseRepository because ProductAttribute has no single Guid PK.
/// </summary>
public interface IProductAttributeRepository
{
    Task<ProductAttribute?> GetAsync(Guid productId, Guid attributeId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid productId, Guid attributeId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByAttributeIdAsync(Guid attributeId, CancellationToken cancellationToken = default);
    Task<List<ProductAttributeResponse>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<List<ProductAttribute>> GetRequiredByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    void Add(ProductAttribute productAttribute);
    void Remove(ProductAttribute productAttribute);
}
