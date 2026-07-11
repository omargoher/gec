using GEC.ApplicationCore.Interfaces.Services;
using GEC.ApplicationCore.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GEC.ApplicationCore;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationCore(this IServiceCollection services)
    {
        services.AddScoped<ITestUserService, TestUserService>();
        services.AddAutoMapper(cfg => { }, typeof(DependencyInjection).Assembly);
        return services;
    }
}