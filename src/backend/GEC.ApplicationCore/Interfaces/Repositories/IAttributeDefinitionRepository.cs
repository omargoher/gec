using GEC.ApplicationCore.DTOs;
using GEC.ApplicationCore.DTOs.Attributes;
using GEC.Domain.Entities;

namespace GEC.ApplicationCore.Interfaces.Repositories;

public interface IAttributeDefinitionRepository : IBaseRepository<AttributeDefinition>
{
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<PagedResult<AttributeDefinitionResponse>> GetPagedAsync(AttributeDefinitionFilterParams filterParams, CancellationToken cancellationToken = default);
}
