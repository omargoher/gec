using GEC.ApplicationCore.DTOs.Products;
using GEC.Domain.Entities;

namespace GEC.ApplicationCore.Interfaces.Repositories;

/// <summary>
/// Repository for ProductAttribute (composite PK: ProductId + AttributeId).
/// Inherits from IBaseRepository to satisfy architectural rules, though GetByIdAsync(Guid) won't work due to composite PK.
/// </summary>
public interface IProductAttributeRepository : IBaseRepository<ProductAttribute>
{
    Task<ProductAttribute?> GetAsync(Guid productId, Guid attributeId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid productId, Guid attributeId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByAttributeIdAsync(Guid attributeId, CancellationToken cancellationToken = default);
    Task<List<ProductAttributeResponse>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<List<ProductAttribute>> GetRequiredByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
}
