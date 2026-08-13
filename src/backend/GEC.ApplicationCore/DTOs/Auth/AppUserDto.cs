namespace GEC.ApplicationCore.DTOs.Auth;

public record AppUserDto(string Id, string Email, IReadOnlyList<string> Roles);