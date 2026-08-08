namespace GEC.ApplicationCore.DTOs.PasswordReset;

public record VerifyPasswordResetCodeRequest(string Email, string Code);