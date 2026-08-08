using GEC.ApplicationCore.DTOs.Auth;

namespace GEC.ApplicationCore.Interfaces.Identity;

public interface IIdentityService
{
    Task<(bool Succeeded, string? UserId, IEnumerable<string> Errors)> CreateUserAsync(
        string email, string password);
    Task<(bool Succeeded, string? UserId, IEnumerable<string> Errors)> CreateExternalUserAsync(
        string email, string name);
    Task<AppUserDto?> FindByEmailAsync(string email);
    Task<AppUserDto?> FindByIdAsync(string userId);
    Task<bool> CheckPasswordAsync(string userId, string password);
    Task AddToRoleAsync(string userId, string role);
    Task<bool> IsLockedOutAsync(string userId);
    Task AccessFailedAsync(string userId);
    Task ResetAccessFailedCountAsync(string userId);
    Task ConfirmEmailAsync(string userId);
    Task<bool> IsEmailConfirmedAsync(string userId);
    Task DeleteUserAsync(
        string userId);

    Task ChangePassword(string userId, string oldPassword, string newPassword);
}