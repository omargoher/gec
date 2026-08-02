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
        var logger = services.GetRequiredService<ILogger<AppIdentityDbContext>>();

        try
        {
            var dbContext = services.GetRequiredService<AppIdentityDbContext>();
            logger.LogInformation("Applying pending database migrations...");
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying database migrations.");
            throw; // Stop application startup if database schema fails to apply
        }
    }
}