using GEC.ApplicationCore.DTOs.Categories;
using GEC.ApplicationCore.Exceptions;
using GEC.ApplicationCore.Interfaces.Repositories;
using GEC.Domain.Entities;
using GEC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GEC.Infrastructure.Repositories;

public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdWithChildrenAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Include(c => c.Children)
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Category?> GetByIdWithActiveChildrenAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .Include(c => c.Children.Where(child => child.IsActive))
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive, cancellationToken);

    }

    public async Task<Category?> GetBySlugWithActiveChildrenAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .Include(c => c.Children.Where(child => child.IsActive))
            .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive, cancellationToken);
    }

    public async Task<List<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .Select(c => new CategoryDto(
                c.Id,
                c.ParentId,
                c.Name,
                c.Slug,
                c.IsActive
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CategoryAdminTreeNodeDto>> GetFlatAdminTreeNodesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .Select(c => new CategoryAdminTreeNodeDto(
                c.Id,
                c.ParentId,
                c.Name,
                c.Slug,
                c.Icon,
                c.ImageUrl,
                c.IsActive,
                new List<CategoryAdminTreeNodeDto>()
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CategoryTreeNodeDto>> GetFlatActiveTreeNodesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .Select(c => new CategoryTreeNodeDto(
                c.Id,
                c.ParentId,
                c.Name,
                c.Slug,
                c.Icon,
                new List<CategoryTreeNodeDto>()
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SlugExistAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AnyAsync(c => c.Slug == slug && (!excludeId.HasValue || c.Id != excludeId.Value), cancellationToken);
    }
}