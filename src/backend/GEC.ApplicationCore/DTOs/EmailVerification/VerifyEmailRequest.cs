namespace GEC.ApplicationCore.DTOs.EmailVerification;

public record VerifyEmailRequest(string Email, string Code);