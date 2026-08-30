using GEC.ApplicationCore.DTOs.Categories;
using GEC.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GEC.API.Controllers;

/// <summary>
/// Product category browsing (public) and management (admin).
/// </summary>
[ApiController]
[Route("api/categories")]
[Tags("Categories")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="201">
    /// Category created. The Location header points to the new category, and the
    /// body contains its full representation (same shape as <see cref="GetByIdAsync"/>).
    /// </response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">
    /// <c>ParentId</c> was supplied but no category exists with that id.
    /// </response>
    /// <response code="409">The slug is already in use by another category.</response>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryResponse>> CreateAsync(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryService.CreateCategoryAsync(request, cancellationToken);
        return CreatedAtRoute("GetCategoryById", new { categoryId = category.Id }, category);
    }

    /// <summary>
    /// Partially updates a category. Only the fields present in the request body are changed.
    /// </summary>
    /// <param name="categoryId"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <remarks>
    /// <c>ParentId</c> and <c>ClearParent: true</c> are mutually exclusive (rejected at
    /// validation). Setting <c>IsActive: false</c> is rejected if the category still has
    /// an active subcategory; setting <c>IsActive: true</c> is rejected if its parent is
    /// currently inactive.
    /// </remarks>
    /// <response code="204">Category updated.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">
    /// The category does not exist, or <c>ParentId</c> was supplied but no category
    /// exists with that id.
    /// </response>
    /// <response code="409">The slug is already in use by another category.</response>
    [Authorize(Roles = "Admin")]
    [HttpPatch("{categoryId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> UpdateAsync(
        Guid categoryId,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        await _categoryService.UpdateCategoryAsync(categoryId, request, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes a category.
    /// </summary>
    /// <param name="categoryId"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="204">Category deleted.</response>
    /// <response code="404">The category does not exist.</response>
    /// <response code="409">The category still has subcategories and cannot be deleted.</response>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{categoryId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> DeleteAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        await _categoryService.DeleteCategoryAsync(categoryId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Checks whether a slug is free to use.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns <c>true</c> if the slug is available, <c>false</c> if it's taken.</response>
    [AllowAnonymous]
    [HttpGet("slug/available")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> CheckSlugAvailabilityAsync(
        [FromBody] CheckSlugAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        var isAvailable = await _categoryService.IsSlugAvailableAsync(request, cancellationToken);
        return Ok(isAvailable);
    }

    /// <summary>
    /// Gets the public category tree: active root categories with their active descendants nested.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Category tree retrieved successfully.</response>
    [AllowAnonymous]
    [HttpGet("tree")]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryTreeNodeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryTreeNodeDto>>> GetTreeAsync(
        CancellationToken cancellationToken = default)
    {
        var tree = await _categoryService.GetCategoryTreeAsync(cancellationToken);
        return Ok(tree);
    }

    /// <summary>
    /// Gets the full category tree for admin management, including inactive categories.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <remarks>
    /// Kept on a distinct path from <see cref="GetTreeAsync"/> rather than the same route —
    /// routing can't disambiguate two actions on an identical route by
    /// <c>[Authorize]</c> alone.
    /// </remarks>
    /// <response code="200">Admin category tree retrieved successfully.</response>
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/tree")]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryAdminTreeNodeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryAdminTreeNodeDto>>> GetAdminTreeAsync(
        CancellationToken cancellationToken = default)
    {
        var tree = await _categoryService.GetCategoryAdminTreeAsync(cancellationToken);
        return Ok(tree);
    }

    /// <summary>
    /// Gets an active category by id, including its breadcrumbs and direct active subcategories.
    /// </summary>
    /// <param name="categoryId"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Category retrieved successfully.</response>
    /// <response code="404">Category was not found (or is inactive).</response>
    [AllowAnonymous]
    [HttpGet("{categoryId:guid}", Name = "GetCategoryById")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryResponse>> GetByIdAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryService.GetCategoryByIdAsync(categoryId, cancellationToken);
        return Ok(category);
    }

    /// <summary>
    /// Gets an active category by slug, including its breadcrumbs and direct active subcategories.
    /// </summary>
    /// <param name="slug"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Category retrieved successfully.</response>
    /// <response code="404">Category was not found (or is inactive).</response>
    [AllowAnonymous]
    [HttpGet("slug/{slug}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryResponse>> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryService.GetCategoryBySlugAsync(slug, cancellationToken);
        return Ok(category);
    }

    /// <summary>
    /// Gets the breadcrumb trail (root → current) for an active category by slug.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Breadcrumbs retrieved successfully.</response>
    /// <response code="404">Category was not found (or is inactive).</response>
    [AllowAnonymous]
    [HttpGet("slug/breadcrumbs")]
    [ProducesResponseType(typeof(List<BreadcrumbDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<BreadcrumbDto>>> GetBreadcrumbsAsync(
        [FromQuery] GetBreadcrumbsRequest request,
        CancellationToken cancellationToken = default)
    {
        var breadcrumbs = await _categoryService.GetBreadcrumbsBySlugAsync(request, cancellationToken);
        return Ok(breadcrumbs);
    }

    /// <summary>
    /// Adds a product to a category.
    /// </summary>
    /// <param name="categoryId"></param>
    /// <param name="productId"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="204">Product added to category.</response>
    /// <response code="404">The category or product does not exist.</response>
    /// <response code="409">The product is already assigned to this category.</response>
    [Authorize(Roles = "Admin")]
    [HttpPost("{categoryId:guid}/products/{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> AddProductAsync(
        Guid categoryId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        await _categoryService.AddProductAsync(categoryId, productId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Removes a product from a category.
    /// </summary>
    /// <param name="categoryId"></param>
    /// <param name="productId"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="204">Product removed from category.</response>
    /// <response code="404">The product is not assigned to this category.</response>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{categoryId:guid}/products/{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveProductAsync(
        Guid categoryId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        await _categoryService.RemoveProductAsync(categoryId, productId, cancellationToken);
        return NoContent();
    }
}