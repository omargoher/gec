using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GEC.Infrastructure.Identity;

public class AppIdentityDbContextFactory
    : IDesignTimeDbContextFactory<AppIdentityDbContext>
{
    public AppIdentityDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../GEC.API");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("pgsql")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'pgsql' was not found.");

        var optionsBuilder = new DbContextOptionsBuilder<AppIdentityDbContext>();

        optionsBuilder.UseNpgsql(connectionString, npgsql =>
        {
            npgsql.MigrationsAssembly(typeof(AppIdentityDbContext).Assembly.FullName);
            npgsql.MigrationsHistoryTable("__EFMigrationsHistory_Identity");
        });

        return new AppIdentityDbContext(optionsBuilder.Options);
    }
}