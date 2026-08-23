# Repository & Unit of Work Guide

This guide details how to implement database persistence using the Repository and Unit of Work patterns with Entity Framework Core.

---

## Pattern Overview

![repository-uow-pattern](../images/repository-uow-pattern.png)

---

## Step 1: Create the Domain Entity
Entities are defined in `GEC.Domain/Entities/` and must inherit from `BaseEntity` in order to automatically obtain a `Guid Id` and audit columns (`CreatedAt`, `UpdatedAt`):

```csharp
using GEC.Domain.Entities;

namespace GEC.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
}
```

---

## Step 2: Configure EF Core Mapping
Use Fluent API to configure schema mappings. Do not use Data Annotations in the Domain entities.
Create a class under `GEC.Infrastructure/Persistence/Configurations/` named `<EntityName>EntityTypeConfiguration.cs`:

```csharp
using GEC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEC.Infrastructure.Persistence.Configurations;

public class CategoryEntityTypeConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(120);

        builder.HasIndex(x => x.Slug)
            .IsUnique();
    }
}
```

Add your entity as a `DbSet` property in `GEC.Infrastructure/Persistence/ApplicationDbContext.cs`:
```csharp
public DbSet<Category> Categories { get; set; }
```
*(All Fluent Configurations in the assembly are registered automatically during `OnModelCreating` via `ApplyConfigurationsFromAssembly`).*

---

## Step 3: Define the Repository Interface
Define the interface in the Application Core under `GEC.ApplicationCore/Interfaces/Repositories/` named `I<EntityName>Repository.cs`. It must inherit from `IBaseRepository<T>`:

```csharp
using GEC.Domain.Entities;

namespace GEC.ApplicationCore.Interfaces.Repositories;

public interface ICategoryRepository : IBaseRepository<Category>
{
    // Define custom queries here (e.g. queries that cannot be solved by base CRUD)
    Task<Category?> GetBySlugAsync(string slug);
}
```

---

## Step 4: Implement the Repository Class
Implement the repository in Infrastructure under `GEC.Infrastructure/Repositories/` named `<EntityName>Repository.cs`. Inherit from `BaseRepository<T>` and implement your interface:

```csharp
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

    public async Task<Category?> GetBySlugAsync(string slug)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Slug == slug);
    }
}
```

---

## Step 5: Register Repository in the Unit of Work

### 1. Update the Interface
Add the repository property to `GEC.ApplicationCore/Interfaces/IUnitOfWork.cs`:
```csharp
using GEC.ApplicationCore.Interfaces.Repositories;

namespace GEC.ApplicationCore.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ITestUserRepository TestUser { get; }
    ICategoryRepository Category { get; } // Add this
    Task<int> SaveChangesAsync();
}
```

### 2. Update the Implementation
Instantiate the repository inside `GEC.Infrastructure/UnitOfWork/UnitOfWork.cs`:
```csharp
using GEC.ApplicationCore.Interfaces;
using GEC.ApplicationCore.Interfaces.Repositories;
using GEC.Infrastructure.Persistence;
using GEC.Infrastructure.Repositories;

namespace GEC.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        TestUser = new TestUserRepository(_context);
        Category = new CategoryRepository(_context); // Add this
    }

    public ITestUserRepository TestUser { get; private set; }
    public ICategoryRepository Category { get; private set; } // Add this

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

---

## Step 6: Create Database Migration
To apply the schema changes, generate a migration in the command terminal under the `src/backend` folder:
```bash
dotnet ef migrations add AddCategoryEntity -p GEC.Infrastructure -s GEC.API -c ApplicationDbContext -o Persistence/Migrations
```
Then run the migration to update the database:
```bash
dotnet ef database update -p GEC.Infrastructure -s GEC.API -c ApplicationDbContext
```
The repository is now fully accessible inside your application services via `_unitOfWork.Category`!
