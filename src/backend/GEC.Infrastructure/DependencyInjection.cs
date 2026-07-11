using GEC.ApplicationCore.Interfaces.Persistence;
using GEC.ApplicationCore.Interfaces.Repositories;
using GEC.ApplicationCore.Options;
using GEC.Infrastructure.Persistence;
using GEC.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GEC.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Configuration
        services.Configure<TestOptions>(configuration.GetSection("TestSettings"));
        var connectionString = configuration.GetConnectionString("pgsql");

        // Register DB Context
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString,
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
        // services.AddScoped<IEmailService, EmailService>();

        services.AddScoped<ITestUserRepository, TestUserRepository>();
        return services;
    }

}