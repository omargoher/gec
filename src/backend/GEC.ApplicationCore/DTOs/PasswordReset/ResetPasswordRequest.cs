namespace GEC.ApplicationCore.DTOs.PasswordReset;

public record ResetPasswordRequest(string Email, string ResetToken, string NewPassword);