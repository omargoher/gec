using GEC.ApplicationCore.DTOs;
using GEC.ApplicationCore.DTOs.EmailVerification;
using GEC.ApplicationCore.Exceptions;
using GEC.ApplicationCore.Interfaces.Identity;
using GEC.ApplicationCore.Interfaces.Services;
using GEC.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace GEC.Infrastructure.Services;

public class EmailVerificationService : IEmailVerificationService
{
    private readonly IOtpManager _otpManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public EmailVerificationService(IOtpManager otpManager, UserManager<ApplicationUser> userManager)
    {
        _otpManager = otpManager;
        _userManager = userManager;
    }

    public async Task SendVerificationCodeAsync(SendVerificationCodeRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            throw new NotFoundException("User");

        if (user.EmailConfirmed)
            throw new InvalidRequestException($"Email {request.Email} is already verified.");

        await _otpManager.SendOtpAsync(request.Email, OtpPurpose.EmailVerification, cancellationToken);
    }

    public async Task VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            throw new NotFoundException("User");

        if (user.EmailConfirmed)
            throw new InvalidRequestException($"Email {request.Email} is already verified.");

        await _otpManager.ValidateOtpAsync(request.Code, request.Email, OtpPurpose.EmailVerification, cancellationToken);

        user.EmailConfirmed = true;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Failed to confirm email");
        }
    }
}