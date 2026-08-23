using GEC.ApplicationCore.DTOs;
using GEC.ApplicationCore.DTOs.Categories;

namespace GEC.ApplicationCore.Interfaces.Services;

// TODO: Return slug recommend in CheckSlugAvailability
// TODO: Add Has Products check in DeleteCategoryAsync
public interface ICategoryService
{
    Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task UpdateCategoryAsync(Guid categoryId, UpdateCategoryRequest request, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default);

    // return also suggest slug
    Task<bool> IsSlugAvailableAsync(CheckSlugAvailabilityRequest request, CancellationToken cancellationToken = default);
    Task<List<BreadcrumbDto>> GetBreadcrumbsBySlugAsync(GetBreadcrumbsRequest request, CancellationToken cancellationToken = default);
    Task<CategoryResponse> GetCategoryByIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<CategoryResponse> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoryTreeNodeDto>> GetCategoryTreeAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoryAdminTreeNodeDto>> GetCategoryAdminTreeAsync(CancellationToken cancellationToken = default);
}