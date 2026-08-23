using GEC.ApplicationCore.Interfaces.Persistence;
using GEC.Domain.Entities;
using GEC.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GEC.Infrastructure.Persistence;

public static class AdminSeeder
{


    public static async Task SeedAdminAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        string name = configuration["AdminCredentials:Name"] ?? "Super Admin";
        string adminEmail = configuration["AdminCredentials:Email"] ?? "admin@example.com";
        string adminPassword = configuration["AdminCredentials:Password"] ?? "Admin@123";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            // Create the admin user
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to seed admin user: {errors}");
            }

            result = await userManager.AddToRoleAsync(adminUser, "Admin");
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to seed admin user: {errors}");
            }

            unitOfWork.Admin.Add(new Admin
            {
                IdentityUserId = adminUser.Id,
                Name = name,
                Email = adminEmail
            });

            await unitOfWork.SaveChangesAsync();
        }
    }
}