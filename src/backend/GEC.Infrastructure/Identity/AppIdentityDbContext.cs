using GEC.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GEC.Infrastructure.Identity;

public class AppIdentityDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
        : base(options)
    {
    }
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

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
            typeof(AppIdentityDbContext).Assembly,
            type => type.Namespace is not null && type.Namespace.StartsWith("GEC.Infrastructure.Identity"));

        // Explicitly convert ASP.NET Identity default table names to snake_case
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            if (tableName != null)
            {
                entity.SetTableName(tableName switch
                {
                    "AspNetUsers" => "asp_net_users",
                    "AspNetRoles" => "asp_net_roles",
                    "AspNetUserClaims" => "asp_net_user_claims",
                    "AspNetUserRoles" => "asp_net_user_roles",
                    "AspNetUserLogins" => "asp_net_user_logins",
                    "RoleClaims" or "AspNetRoleClaims" => "asp_net_role_claims",
                    "AspNetUserTokens" => "asp_net_user_tokens",
                    _ => entity.GetTableName()
                });
            }
        }
    }

}