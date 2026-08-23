
using GEC.ApplicationCore.Interfaces.Repositories;

namespace GEC.ApplicationCore.Interfaces.Persistence;

public interface IUnitOfWork : IDisposable
{
    ITestUserRepository TestUser { get; }
    IAdminRepository Admin { get; }
    ICustomerRepository Customer { get; }
    ICategoryRepository Category { get; }

    // Catalog repositories
    IProductRepository Products { get; }
    IProductVariantRepository ProductVariants { get; }
    IAttributeDefinitionRepository AttributeDefinitions { get; }
    IAttributeValueRepository AttributeValues { get; }
    IProductAttributeRepository ProductAttributes { get; }
    IVariantAttributeValueRepository VariantAttributeValues { get; }
    IProductSpecificationRepository ProductSpecifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the current xmin (optimistic concurrency token) for a tracked entity.
    /// Call after SaveChangesAsync to get the updated token.
    /// </summary>
    uint GetRowVersion<T>(T entity) where T : class;
}