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
        string email, string password, string name)
    {
        var user = new ApplicationUser { Email = email, Name = name, UserName = email };
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
            Name: user.Name,
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
            Name: user.Name,
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
        await _userManager.AddToRoleAsync(user, role);
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
}