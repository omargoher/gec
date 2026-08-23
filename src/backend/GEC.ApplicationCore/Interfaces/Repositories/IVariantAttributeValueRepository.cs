using GEC.Domain.Entities;

namespace GEC.ApplicationCore.Interfaces.Repositories;

/// <summary>
/// Repository for VariantAttributeValue (composite PK: VariantId + AttributeId).
/// Inherits from IBaseRepository, but GetByIdAsync(Guid) is invalid due to composite PK.
/// </summary>
public interface IVariantAttributeValueRepository : IBaseRepository<VariantAttributeValue>
{
    Task<VariantAttributeValue?> GetAsync(Guid variantId, Guid attributeId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid variantId, Guid attributeId, CancellationToken cancellationToken = default);
    Task<bool> ExistsForProductAttributeAsync(Guid productId, Guid attributeId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByAttributeValueIdAsync(Guid attributeValueId, CancellationToken cancellationToken = default);
    Task<List<VariantAttributeValue>> GetByVariantIdAsync(Guid variantId, CancellationToken cancellationToken = default);
}
