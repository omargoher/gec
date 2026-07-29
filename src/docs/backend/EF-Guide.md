# Entity Framework Guide

## Add Entity to EF
- We set the entities in the Domain layer, so if you want to add a new entity to EF, you must add it to the Domain layer first (inheriting from `BaseEntity`).
- Add your entity as a `DbSet<EntityName>` property in the `ApplicationDbContext` class in the Infrastructure layer in [ApplicationDbContext.cs](../../backend/GEC.Infrastructure/Persistence/ApplicationDbContext.cs).
- We use Fluent API to configure the entity in a separate configuration class under the Infrastructure layer in `src/backend/GEC.Infrastructure/Persistence/Configurations/` folder.
  - Create a new class for your entity and name it `<EntityName>EntityTypeConfiguration.cs`.
  - Implement the `IEntityTypeConfiguration<T>` interface and override the `Configure` method to configure the entity properties.
Like this:
```csharp
using GEC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEC.Infrastructure.Persistence.Configurations;

public class TestUserEntityTypeConfiguration : IEntityTypeConfiguration<TestUser>
{
    public void Configure(EntityTypeBuilder<TestUser> builder)
    {
        builder.ToTable("test_users");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);
    }
}
```

## Migrations
- After adding the entity to the `DbContext` and configuring it, you need to create a migration to update the database schema.
- Our migrations are located in the `src/backend/GEC.Infrastructure/Persistence/Migrations/` folder.
- To create a migration, run the following command in your terminal (make sure your current directory is `src/backend`):
```bash
dotnet ef migrations add <MigrationName> -p GEC.Infrastructure -s GEC.API -c ApplicationDbContext -o Persistence/Migrations
```
- Replace `<MigrationName>` with a descriptive name for your migration, such as `AddTestUserEntity`.

- After creating the migration, apply it to the database by running:
```bash
dotnet ef database update -p GEC.Infrastructure -s GEC.API -c ApplicationDbContext
```
- This command applies pending migrations to the database and updates the schema accordingly.