using GEC.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GEC.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<TestUser> TestUsers { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Address> Addresses { get; set; }

    public DbSet<Category> Categories { get; set; }

    // Catalog entities
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<AttributeDefinition> AttributeDefinitions { get; set; }
    public DbSet<AttributeValue> AttributeValues { get; set; }
    public DbSet<ProductAttribute> ProductAttributes { get; set; }
    public DbSet<VariantAttributeValue> VariantAttributeValues { get; set; }
    public DbSet<ProductSpecification> ProductSpecifications { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply Fluent Configurations
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly,
            type => type.Namespace is not null && type.Namespace.StartsWith("GEC.Infrastructure.Persistence"));
    }

    // Auto Set Time => For CreatedAt and UpdatedAt
    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}