namespace GEC.ApplicationCore.DTOs.Auth;

public record AuthResponse(
    string AccessToken,
    string RefreshToken);