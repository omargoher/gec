using GEC.ApplicationCore.Interfaces.Repositories;
using GEC.Domain.Entities;
using GEC.Infrastructure.Persistence;

namespace GEC.Infrastructure.Repositories;

public class ProductSpecificationRepository : BaseRepository<ProductSpecification>, IProductSpecificationRepository
{
    private readonly ApplicationDbContext _context;

    public ProductSpecificationRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }
}
