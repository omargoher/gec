using GEC.ApplicationCore.DTOs;
using GEC.ApplicationCore.DTOs.Attributes;

namespace GEC.ApplicationCore.Interfaces.Services;

/// <summary>
/// Application service for global AttributeDefinition and AttributeValue management.
/// Covers all use cases defined in Catalog PDR v3.1 §4 and §5.6–5.7.
/// </summary>
public interface IAttributeService
{
    Task<AttributeDefinitionResponse> AddAttributeDefinitionAsync(CreateAttributeDefinitionRequest request, CancellationToken cancellationToken = default);
    Task<AttributeDefinitionResponse> RemoveAttributeDefinitionAsync(Guid attributeId, CancellationToken cancellationToken = default);
    Task<PagedResult<AttributeDefinitionResponse>> GetAttributeDefinitionsAsync(AttributeDefinitionFilterParams filterParams, CancellationToken cancellationToken = default);

    Task<AttributeValueResponse> AddAttributeValueAsync(Guid attributeId, CreateAttributeValueRequest request, CancellationToken cancellationToken = default);
    Task<AttributeValueResponse> RemoveAttributeValueAsync(Guid attributeId, Guid valueId, CancellationToken cancellationToken = default);
    Task<List<AttributeValueResponse>> GetAttributeValuesByAttributeIdAsync(Guid attributeId, CancellationToken cancellationToken = default);
}
