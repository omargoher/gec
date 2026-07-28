namespace GEC.ApplicationCore.DTOs.Auth;

public record RegisterRequest(string Email, string Password, string FirstName, string LastName);