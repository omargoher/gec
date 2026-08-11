using GEC.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GEC.Infrastructure.Persistence;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrationsAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("DatabaseMigrations");

        try
        {
            logger.LogInformation("Applying pending migrations for AppIdentityDbContext...");
            var identityDbContext = services.GetRequiredService<AppIdentityDbContext>();
            await identityDbContext.Database.MigrateAsync();
            logger.LogInformation("AppIdentityDbContext migrations applied successfully.");

            logger.LogInformation("Applying pending migrations for ApplicationDbContext...");
            var appDbContext = services.GetRequiredService<ApplicationDbContext>();
            await appDbContext.Database.MigrateAsync();
            logger.LogInformation("ApplicationDbContext migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying database migrations.");
            throw; // Stop application startup if database schema fails to apply
        }
    }
}