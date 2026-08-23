using GEC.Domain.Entities;

namespace GEC.ApplicationCore.Interfaces.Repositories;

/// <summary>
/// Repository for VariantAttributeValue (composite PK: VariantId + AttributeId).
/// Does not inherit IBaseRepository because VariantAttributeValue has no single Guid PK.
/// </summary>
public interface IVariantAttributeValueRepository
{
    Task<VariantAttributeValue?> GetAsync(Guid variantId, Guid attributeId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid variantId, Guid attributeId, CancellationToken cancellationToken = default);
    Task<bool> ExistsForProductAttributeAsync(Guid productId, Guid attributeId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByAttributeValueIdAsync(Guid attributeValueId, CancellationToken cancellationToken = default);
    Task<List<VariantAttributeValue>> GetByVariantIdAsync(Guid variantId, CancellationToken cancellationToken = default);
    void Add(VariantAttributeValue variantAttributeValue);
    void Remove(VariantAttributeValue variantAttributeValue);
}
