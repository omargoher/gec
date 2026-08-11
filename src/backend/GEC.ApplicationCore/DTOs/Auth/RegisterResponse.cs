namespace GEC.ApplicationCore.DTOs.Auth;

public record RegisterResponse
(string Id, string Email, string Name, IReadOnlyList<string> Roles);