using GEC.ApplicationCore.DTOs;
using GEC.ApplicationCore.DTOs.PasswordReset;
using GEC.ApplicationCore.Exceptions;
using GEC.ApplicationCore.Interfaces.Services;
using GEC.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace GEC.Infrastructure.Services;

public class PasswordResetService : IPasswordResetService
{
    private readonly IOtpManager _otpManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public PasswordResetService(IOtpManager otpManager, UserManager<ApplicationUser> userManager)
    {
        _otpManager = otpManager;
        _userManager = userManager;
    }

    public async Task SendPasswordResetCodeAsync(SendPasswordResetCodeRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            throw new NotFoundException("User");

        await _otpManager.SendOtpAsync(request.Email, OtpPurpose.PasswordReset, cancellationToken);
    }

    public async Task<VerifyPasswordResetCodeResponse> VerifyPasswordResetCodeAsync(VerifyPasswordResetCodeRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            throw new NotFoundException("User");

        await _otpManager.ValidateOtpAsync(request.Code, request.Email, OtpPurpose.PasswordReset, cancellationToken);

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        return new VerifyPasswordResetCodeResponse(token);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            throw new NotFoundException("User");

        var result = await _userManager.ResetPasswordAsync(user, request.ResetToken, request.NewPassword);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Password reset failed.");
        }
    }
}