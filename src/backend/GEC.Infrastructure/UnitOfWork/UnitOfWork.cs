using GEC.ApplicationCore.Exceptions;
using GEC.ApplicationCore.Interfaces.Persistence;
using GEC.ApplicationCore.Interfaces.Repositories;
using GEC.Infrastructure.Persistence;
using GEC.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GEC.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        TestUser = new TestUserRepository(_context);
        Admin = new AdminRepository(_context);
        Customer = new CustomerRepository(_context);
        Category = new CategoryRepository(_context);

        // Catalog repositories
        Products = new ProductRepository(_context);
        ProductVariants = new ProductVariantRepository(_context);
        AttributeDefinitions = new AttributeDefinitionRepository(_context);
        AttributeValues = new AttributeValueRepository(_context);
        ProductAttributes = new ProductAttributeRepository(_context);
        VariantAttributeValues = new VariantAttributeValueRepository(_context);
        ProductSpecifications = new ProductSpecificationRepository(_context);
    }

    public ITestUserRepository TestUser { get; private set; }
    public IAdminRepository Admin { get; private set; }
    public ICustomerRepository Customer { get; private set; }
    public ICategoryRepository Category { get; private set; }

    // Catalog
    public IProductRepository Products { get; private set; }
    public IProductVariantRepository ProductVariants { get; private set; }
    public IAttributeDefinitionRepository AttributeDefinitions { get; private set; }
    public IAttributeValueRepository AttributeValues { get; private set; }
    public IProductAttributeRepository ProductAttributes { get; private set; }
    public IVariantAttributeValueRepository VariantAttributeValues { get; private set; }
    public IProductSpecificationRepository ProductSpecifications { get; private set; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException();
        }
    }

    public uint GetRowVersion<T>(T entity) where T : class
        => _context.Entry(entity).Property<uint>("xmin").CurrentValue;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null) return;
        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null) return;
        try
        {
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction != null)
                await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public void Dispose() => _context.Dispose();
}
