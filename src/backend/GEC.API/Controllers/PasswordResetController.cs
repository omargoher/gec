using GEC.ApplicationCore.DTOs.PasswordReset;
using GEC.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GEC.API.Controllers;

/// <summary>
/// Handles password reset requests.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Password Reset")]
public class PasswordResetController : ControllerBase
{
    private readonly IPasswordResetService _passwordResetService;

    public PasswordResetController(IPasswordResetService passwordResetService)
    {
        _passwordResetService = passwordResetService;
    }

    /// <summary>
    /// Initiates a password reset by sending a one-time reset code (OTP) to the user's email.
    /// </summary>
    /// <param name="request">The target email address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="203">Password reset code sent successfully.</response>
    /// <response code="400">Invalid payload format.</response>
    /// <response code="404">User with the specified email was not found.</response>
    [HttpPost("send-code")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status203NonAuthoritative)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendPasswordResetCodeAsync([FromBody] SendPasswordResetCodeRequest request, CancellationToken cancellationToken = default)
    {
        await _passwordResetService.SendPasswordResetCodeAsync(request, cancellationToken);
        return Accepted();
    }

    /// <summary>
    /// Validates the received password reset code (OTP) and issues a short-lived reset token.
    /// </summary>
    /// <param name="request">The email and the verification code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A password reset token needed for the final reset step.</returns>
    /// <response code="200">OTP verified successfully, returns the reset token.</response>
    /// <response code="400">Invalid, expired, or previously used code.</response>
    /// <response code="404">User with the specified email was not found.</response>
    [HttpPost("verify-code")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(VerifyPasswordResetCodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ValidatePasswordResetCodeAsync([FromBody] VerifyPasswordResetCodeRequest request, CancellationToken cancellationToken = default)
    {
        var resetToken = await _passwordResetService.VerifyPasswordResetCodeAsync(request, cancellationToken);
        return Ok(new { ResetToken = resetToken });
    }

    /// <summary>
    /// Resets the user's password using the generated reset token.
    /// </summary>
    /// <param name="request">The email, reset token, and new password payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on successful password update.</returns>
    /// <response code="204">Password reset successfully.</response>
    /// <response code="400">Invalid token, password complexity failure, or invalid request payload.</response>
    /// <response code="404">User with the specified email was not found.</response>
    [HttpPost("reset")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        await _passwordResetService.ResetPasswordAsync(request, cancellationToken);
        return NoContent();
    }
}