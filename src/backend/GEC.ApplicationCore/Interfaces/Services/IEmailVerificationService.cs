using GEC.ApplicationCore.DTOs.EmailVerification;

namespace GEC.ApplicationCore.Interfaces.Services;

public interface IEmailVerificationService
{
    Task SendVerificationCodeAsync(SendVerificationCodeRequest request, CancellationToken cancellationToken);
    Task VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken);
}