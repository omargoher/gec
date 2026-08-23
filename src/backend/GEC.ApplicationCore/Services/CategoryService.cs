using GEC.ApplicationCore.DTOs;
using GEC.ApplicationCore.DTOs.Categories;
using GEC.ApplicationCore.Exceptions;
using GEC.ApplicationCore.Interfaces.Persistence;
using GEC.ApplicationCore.Interfaces.Repositories;
using GEC.ApplicationCore.Interfaces.Services;
using GEC.Domain.Entities;

namespace GEC.ApplicationCore.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var slug = request.Slug.ToLowerInvariant();

        if (await _unitOfWork.Category.SlugExistAsync(slug, null, cancellationToken))
        {
            throw new ConflictException("Category slug");
        }

        if (request.ParentId.HasValue)
        {
            await ValidateParentCategoryAsync(null, request.ParentId.Value, request.IsActive, cancellationToken);
        }

        var category = new Category
        {
            ParentId = request.ParentId,
            Name = request.Name,
            Slug = slug,
            Description = request.Description,
            Icon = request.Icon,
            ImageUrl = request.ImageUrl,
            IsActive = request.IsActive
        };

        _unitOfWork.Category.Add(category);

        // this may return exception for slug unique constrain (if send 2 request in same time) try and catch it 
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CategoryResponse
        (
            Id: category.Id,
            ParentId: category.ParentId,
            Name: category.Name,
            Slug: category.Slug,
            Description: category.Description,
            Icon: category.Icon,
            ImageUrl: category.ImageUrl,
            Breadcrumbs: await GetBreadcrumbsBySlugAsync(new GetBreadcrumbsRequest(category.Slug), cancellationToken),
            SubCategories: new List<SubCategoryDto>()
        );
    }

    public async Task UpdateCategoryAsync(Guid categoryId, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Category.GetByIdWithChildrenAsync(categoryId, cancellationToken);

        if (category == null)
        {
            throw new NotFoundException("Category");
        }

        var slug = request.Slug?.ToLowerInvariant();
        if (slug != null && await _unitOfWork.Category.SlugExistAsync(slug, category.Id, cancellationToken))
        {
            throw new ConflictException("Category slug");
        }

        if (request.ClearParent.HasValue && request.ClearParent.Value)
        {
            category.ParentId = null;
        }
        else if (request.ParentId != null && request.ParentId != category.ParentId)
        {
            await ValidateParentCategoryAsync(category.Id, request.ParentId.Value, request.IsActive ?? category.IsActive, cancellationToken);
        }

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
            {
                // if IsActive change to True and the parent not change so should sure the curent parent is Active
                if ((request.ParentId == null || request.ParentId == category.ParentId) && category.ParentId.HasValue)
                {
                    var parent = await _unitOfWork.Category.GetByIdAsync(category.ParentId.Value, cancellationToken);
                    if (parent != null && parent.IsActive == false)
                    {
                        throw new InvalidOperationException("Cannot active category if the parent in inactive.");
                    }
                }
            }
            else
            {
                // if IsActive change to false should sure the all children is inactive
                foreach (var child in category.Children)
                {
                    if (child.IsActive)
                    {
                        throw new InvalidOperationException("can not deactive category has active subcategory");
                    }
                }

            }
        }

        category.Name = request.Name ?? category.Name;
        category.Slug = slug ?? category.Slug;
        category.Description = request.Description ?? category.Description;
        category.ParentId =
            request.ClearParent.HasValue
            && request.ClearParent.Value
            ? null
            : request.ParentId ?? category.ParentId;
        category.IsActive = request.IsActive ?? category.IsActive;
        category.Icon = request.Icon ?? category.Icon;
        category.ImageUrl = request.ImageUrl ?? category.ImageUrl;

        _unitOfWork.Category.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IsSlugAvailableAsync(CheckSlugAvailabilityRequest request, CancellationToken cancellationToken = default)
    {
        var slug = request.Slug?.ToLowerInvariant();
        if (await _unitOfWork.Category.SlugExistAsync(slug, request.ExcludeCategoryId, cancellationToken))
        {
            return false;
        }
        return true;
    }

    public async Task<List<BreadcrumbDto>> GetBreadcrumbsBySlugAsync(GetBreadcrumbsRequest request, CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Category.GetAllAsync(cancellationToken);

        // load the categories on memory => O(1) access
        // load all categories to dictionary take O(n)
        var categoryById = categories.ToDictionary(c => c.Id);

        // this take O(n)
        var current = categories.FirstOrDefault(c => string.Equals(c.Slug, request.Slug, StringComparison.OrdinalIgnoreCase));

        if (current == null)
        {
            throw new NotFoundException("Category");
        }

        var breadcrumbs = new List<BreadcrumbDto>();

        // this take O(d)
        while (current != null)
        {
            breadcrumbs.Add(new BreadcrumbDto
            (
                Id: current.Id,
                Name: current.Name,
                Slug: current.Slug
            ));

            // this take O(1)
            current = current.ParentId.HasValue ? categoryById.GetValueOrDefault(current.ParentId.Value) : null;
        }

        // O(d) d: is the depth in worst case is 7 
        breadcrumbs.Reverse();
        return breadcrumbs;
    }

    public async Task<CategoryResponse> GetCategoryByIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Category.GetByIdWithActiveChildrenAsync(categoryId, cancellationToken);

        if (category == null)
            throw new NotFoundException("Category");

        return new CategoryResponse
        (
            Id: category.Id,
            ParentId: category.ParentId,
            Name: category.Name,
            Slug: category.Slug,
            Description: category.Description,
            ImageUrl: category.ImageUrl,
            Icon: category.Icon,
            Breadcrumbs: await GetBreadcrumbsBySlugAsync(new GetBreadcrumbsRequest(category.Slug), cancellationToken),
            SubCategories: category.Children.Select(child => new SubCategoryDto
            (
                Id: child.Id,
                Name: child.Name,
                Slug: child.Slug,
                Icon: child.Icon,
                ImageUrl: child.ImageUrl
            )).ToList()
        );
    }

    public async Task<CategoryResponse> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Category.GetBySlugWithActiveChildrenAsync(slug.ToLowerInvariant(), cancellationToken);

        if (category == null)
            throw new NotFoundException("Category");

        return new CategoryResponse
        (
            Id: category.Id,
            ParentId: category.ParentId,
            Name: category.Name,
            Slug: category.Slug,
            Description: category.Description,
            ImageUrl: category.ImageUrl,
            Icon: category.Icon,
            Breadcrumbs: await GetBreadcrumbsBySlugAsync(new GetBreadcrumbsRequest(category.Slug), cancellationToken),
            SubCategories: category.Children.Select(child => new SubCategoryDto
            (
                Id: child.Id,
                Name: child.Name,
                Slug: child.Slug,
                Icon: child.Icon,
                ImageUrl: child.ImageUrl
            )).ToList()
        );
    }

    public async Task<IReadOnlyList<CategoryTreeNodeDto>> GetCategoryTreeAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Category.GetFlatActiveTreeNodesAsync(cancellationToken);

        var categoriesById = categories.ToDictionary(c => c.Id);

        var categoryTree = new List<CategoryTreeNodeDto>();

        foreach (var node in categories)
        {
            if (node.ParentId == null)
            {
                categoryTree.Add(node);
            }
            else
            {
                if (categoriesById.TryGetValue(node.ParentId.Value, out var parent))
                {
                    parent.Children.Add(node);
                }
            }
        }

        return categoryTree;
    }

    public async Task<IReadOnlyList<CategoryAdminTreeNodeDto>> GetCategoryAdminTreeAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Category.GetFlatAdminTreeNodesAsync(cancellationToken);

        var categoriesById = categories.ToDictionary(c => c.Id);

        var categoryTree = new List<CategoryAdminTreeNodeDto>();

        foreach (var node in categories)
        {
            if (node.ParentId == null)
            {
                categoryTree.Add(node);
            }
            else
            {
                if (categoriesById.TryGetValue(node.ParentId.Value, out var parent))
                {
                    parent.Children.Add(node);
                }
            }
        }

        return categoryTree;
    }

    public async Task DeleteCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Category.GetByIdWithChildrenAsync(categoryId, cancellationToken);

        if (category == null)
        {
            throw new NotFoundException("Category");
        }

        // if category has children or products can not deleted
        if (category.Children.Any())
        {
            throw new InvalidOperationException($"Cannot delete '{category.Name}' because it has {category.Children.Count} subcategories. Please delete or move the subcategories first.");
        }

        _unitOfWork.Category.Remove(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidateParentCategoryAsync(Guid? currentCategoryId, Guid newParentId, bool currentCategoryStatus, CancellationToken cancellationToken = default)
    {
        const int maxAllowedDepth = 7;

        // Not apply it to create method
        if (currentCategoryId.HasValue && currentCategoryId.Value == newParentId)
        {
            throw new InvalidOperationException("A category cannot be its own parent.");
        }

        var categories = await _unitOfWork.Category.GetAllAsync(cancellationToken);
        var categoryById = categories.ToDictionary(c => c.Id);

        if (!categoryById.ContainsKey(newParentId))
        {
            throw new NotFoundException("Parent category");
        }

        if (currentCategoryStatus && categoryById[newParentId].IsActive == false)
        {
            throw new InvalidOperationException("Cannot set inactive parent category to an active category.");
        }

        if (currentCategoryId.HasValue && IsDescendant(currentCategoryId.Value, newParentId, categoryById))
        {
            throw new InvalidOperationException("Cannot set parent category to one of its own descendants (circular reference).");
        }

        // Calculate Depth Constraint (Parent Depth + Subtree Height <= 7)
        int parentDepth = GetDepth(newParentId, categoryById);
        int subtreeHeight = currentCategoryId.HasValue ? GetSubtreeHeight(currentCategoryId.Value, categories) : 0;

        // Total depth after move = parentDepth + 1 (current category) + subtreeHeight
        if (parentDepth + 1 + subtreeHeight > maxAllowedDepth)
        {
            throw new InvalidOperationException($"Exceeds the maximum allowed category nesting depth of {maxAllowedDepth} levels.");
        }
    }

    // Checks if targetId is inside the branch under ancestorId
    private static bool IsDescendant(Guid ancestorId, Guid targetId, IReadOnlyDictionary<Guid, CategoryDto> categoryById)
    {
        var currentId = (Guid?)targetId;
        var visited = new HashSet<Guid>();

        while (currentId.HasValue && visited.Add(currentId.Value))
        {
            if (!categoryById.TryGetValue(currentId.Value, out var current))
                break;

            if (current.ParentId == ancestorId)
                return true;

            currentId = current.ParentId;
        }

        return false;
    }

    // Calculates how many levels exist above a category (Root = 1)
    private static int GetDepth(Guid categoryId, IReadOnlyDictionary<Guid, CategoryDto> categoryById)
    {
        int depth = 0;
        var currentId = (Guid?)categoryId;
        var visited = new HashSet<Guid>();

        while (currentId.HasValue && visited.Add(currentId.Value))
        {
            depth++;
            if (categoryById.TryGetValue(currentId.Value, out var category))
            {
                currentId = category.ParentId;
            }
            else
            {
                break;
            }
        }

        return depth;
    }

    // Calculates max height of all branches under a category (No children = 0)
    private static int GetSubtreeHeight(Guid categoryId, IReadOnlyList<CategoryDto> allCategories)
    {
        var childrenByParent = allCategories
            .Where(c => c.ParentId.HasValue)
            .ToLookup(c => c.ParentId!.Value);

        var maxHight = 0;

        var queue = new Queue<(Guid, int)>();
        var visited = new HashSet<Guid>();

        queue.Enqueue((categoryId, 0));
        visited.Add(categoryId);

        while (queue.Any())
        {
            var current = queue.Dequeue();
            maxHight = Math.Max(maxHight, current.Item2);

            var children = childrenByParent[current.Item1];
            foreach (var child in children)
            {
                if (visited.Add(child.Id))
                {
                    queue.Enqueue((child.Id, current.Item2 + 1));
                }
            }
        }

        return maxHight;
    }
}