namespace GEC.ApplicationCore.DTOs.Auth;

public record AppUserDto(string Id, string Email, string Name, IReadOnlyList<string> Roles);