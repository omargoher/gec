using GEC.ApplicationCore.DTOs.Auth;
using GEC.ApplicationCore.Exceptions;
using GEC.ApplicationCore.Interfaces.Identity;
using Microsoft.AspNetCore.Identity;

namespace GEC.Infrastructure.Identity.Service;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<(bool Succeeded, string? UserId, IEnumerable<string> Errors)> CreateUserAsync(
        string email, string password)
    {
        var user = new ApplicationUser { Email = email, UserName = email };
        var result = await _userManager.CreateAsync(user, password);

        return (
            result.Succeeded,
            result.Succeeded ? user.Id : null,
            result.Errors.Select(e => e.Description)
        );
    }

    public async Task<AppUserDto?> FindByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        return new AppUserDto(
            Id: user.Id,
            Email: user.Email!,
            Roles: roles.ToList().AsReadOnly()
        );
    }

    public async Task<AppUserDto?> FindByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        return new AppUserDto(
            Id: user.Id,
            Email: user.Email!,
            Roles: roles.ToList().AsReadOnly()
        );
    }

    public async Task<bool> CheckPasswordAsync(string userId, string password)
    {
        var user = await GetUserOrThrowAsync(userId);
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task AddToRoleAsync(string userId, string role)
    {
        var user = await GetUserOrThrowAsync(userId);
        var result = await _userManager.AddToRoleAsync(user, role);

        // this is violate the Consistency .. because in another methods not throw exceptions when fails
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to assign role '{role}' to user '{userId}'. " +
                $"Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }

    public async Task<bool> IsLockedOutAsync(string userId)
    {
        var user = await GetUserOrThrowAsync(userId);
        return await _userManager.IsLockedOutAsync(user);
    }

    public async Task AccessFailedAsync(string userId)
    {
        var user = await GetUserOrThrowAsync(userId);
        await _userManager.AccessFailedAsync(user);
    }

    public async Task ResetAccessFailedCountAsync(string userId)
    {
        var user = await GetUserOrThrowAsync(userId);
        await _userManager.ResetAccessFailedCountAsync(user);
    }

    public async Task ConfirmEmailAsync(string userId)
    {
        var user = await GetUserOrThrowAsync(userId);
        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);
    }

    public async Task<bool> IsEmailConfirmedAsync(string userId)
    {
        var user = await GetUserOrThrowAsync(userId);
        return await _userManager.IsEmailConfirmedAsync(user);
    }

    private async Task<ApplicationUser> GetUserOrThrowAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User");

        return user;
    }
    public async Task DeleteUserAsync(
        string userId)
    {
        var user = await GetUserOrThrowAsync(userId);

        var result = await _userManager.DeleteAsync(user);

        // this is violate the Consistency .. because in another methods not throw exceptions when fails
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                "Failed to delete user during registration rollback.");
        }
    }

    public async Task ChangePassword(string userId, string oldPassword, string newPassword)
    {
        var user = await GetUserOrThrowAsync(userId);

        var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
    }
}