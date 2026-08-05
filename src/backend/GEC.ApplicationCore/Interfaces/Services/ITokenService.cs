using GEC.ApplicationCore.DTOs.Auth;

namespace GEC.ApplicationCore.Interfaces.Services;

public interface ITokenService
{
    string GenerateAccessToken(AppUserDto user);
    string GenerateRefreshToken();
    string HashToken(string rawToken);
}