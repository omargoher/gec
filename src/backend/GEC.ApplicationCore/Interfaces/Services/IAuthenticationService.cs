using GEC.ApplicationCore.DTOs.Auth;

namespace GEC.ApplicationCore.Interfaces.Services;

public interface IAuthenticationService
{
    Task<AppUserDto> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    public Task<AuthResponse>
        LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    public Task<AuthResponse>
        RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);

    public Task
        LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes every active refresh token for the user, ending all sessions
    /// on all devices (e.g. after a password change, or an explicit
    /// "log out everywhere").
    /// </summary>
    public Task
        LogoutAllAsync(string userId, CancellationToken cancellationToken = default);

}