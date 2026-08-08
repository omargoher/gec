using GEC.ApplicationCore.DTOs.EmailVerification;
using GEC.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GEC.API.Controllers;

/// <summary>
/// Handles email verification and OTP code resending requests.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Email Verification")]
public class EmailVerificationController : ControllerBase
{
    private readonly IEmailVerificationService _emailVerificationService;

    public EmailVerificationController(IEmailVerificationService emailVerificationService)
    {
        _emailVerificationService = emailVerificationService;
    }

    /// <summary>
    /// Verifies a user's email using a one-time verification code (OTP).
    /// </summary>
    /// <param name="request">The email and the verification code.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Email verified successfully.</response>
    /// <response code="400">Invalid verification code, or email is already verified.</response>
    /// <response code="404">User with the specified email was not found.</response>
    [HttpPost("verify")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyAsync([FromBody] VerifyEmailRequest request, CancellationToken cancellationToken = default)
    {
        await _emailVerificationService.VerifyEmailAsync(request, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Resends a verification OTP code to the user's email address.
    /// </summary>
    /// <param name="request">The email to receive the verification code.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Verification code sent successfully.</response>
    /// <response code="400">Email is already verified.</response>
    /// <response code="404">User with the specified email was not found.</response>
    [HttpPost("resend")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResendOtpAsync([FromBody] SendVerificationCodeRequest request, CancellationToken cancellationToken = default)
    {
        await _emailVerificationService.SendVerificationCodeAsync(request, cancellationToken);
        return NoContent();
    }
}