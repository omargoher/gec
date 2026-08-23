using GEC.ApplicationCore.Interfaces.Repositories;
using GEC.Domain.Entities;
using GEC.Infrastructure.Persistence;

namespace GEC.Infrastructure.Repositories;

public class ProductSpecificationRepository : IProductSpecificationRepository
{
    private readonly ApplicationDbContext _context;

    public ProductSpecificationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Add(ProductSpecification specification)
        => _context.ProductSpecifications.Add(specification);
}
