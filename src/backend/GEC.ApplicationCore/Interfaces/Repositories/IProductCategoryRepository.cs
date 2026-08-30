using GEC.ApplicationCore.DTOs.Categories;
using GEC.Domain.Entities;

namespace GEC.ApplicationCore.Interfaces.Repositories;

public interface IProductCategoryRepository : IBaseRepository<ProductCategory>
{
    Task<ProductCategory?> GetAsync(Guid productId, Guid categoryId,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid productId, Guid categoryId, CancellationToken cancellationToken = default);
}