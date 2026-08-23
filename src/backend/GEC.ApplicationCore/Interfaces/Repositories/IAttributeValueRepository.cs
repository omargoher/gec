using GEC.ApplicationCore.DTOs.Attributes;
using GEC.Domain.Entities;

namespace GEC.ApplicationCore.Interfaces.Repositories;

public interface IAttributeValueRepository : IBaseRepository<AttributeValue>
{
    Task<bool> ExistsByAttributeIdAsync(Guid attributeId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid attributeId, string value, CancellationToken cancellationToken = default);
    Task<List<AttributeValueResponse>> GetByAttributeIdAsync(Guid attributeId, CancellationToken cancellationToken = default);
}
