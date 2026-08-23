using GEC.ApplicationCore.DTOs.Categories;
using GEC.Domain.Entities;

namespace GEC.ApplicationCore.Interfaces.Repositories;

public interface ICategoryRepository : IBaseRepository<Category>
{
    Task<Category?> GetByIdWithChildrenAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Category?> GetByIdWithActiveChildrenAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Category?> GetBySlugWithActiveChildrenAsync(string slug, CancellationToken cancellationToken = default);
    Task<List<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<CategoryAdminTreeNodeDto>> GetFlatAdminTreeNodesAsync(CancellationToken cancellationToken = default);
    Task<List<CategoryTreeNodeDto>> GetFlatActiveTreeNodesAsync(CancellationToken cancellationToken = default);
    Task<bool> SlugExistAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
}