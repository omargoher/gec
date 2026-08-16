using GEC.ApplicationCore.DTOs.PasswordReset;

namespace GEC.ApplicationCore.Interfaces.Services;

public interface IPasswordResetService
{
    Task SendPasswordResetCodeAsync(SendPasswordResetCodeRequest request,
        CancellationToken cancellationToken = default);

    Task<VerifyPasswordResetCodeResponse> VerifyPasswordResetCodeAsync(VerifyPasswordResetCodeRequest request,
        CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);

}