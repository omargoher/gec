using GEC.ApplicationCore.DTOs.EmailVerification;
using GEC.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GEC.API.Controllers;

/// <summary>
/// Confirms a user's email address via a one-time verification code (OTP).
/// </summary>
[ApiController]
[Route("api/email-verification")]
[Tags("Email Verification")]
public class EmailVerificationController : ControllerBase
{
    private readonly IEmailVerificationService _emailVerificationService;

    public EmailVerificationController(IEmailVerificationService emailVerificationService)
    {
        _emailVerificationService = emailVerificationService;
    }

    /// <summary>
    /// Confirms an email address using a previously issued verification code.
    /// </summary>
    /// <param name="request">The email and the verification code.</param>
    /// <param name="cancellationToken"></param>
    /// <remarks>
    /// On success, the user's <c>EmailConfirmed</c> flag is set to true. There is no
    /// request body echo or resource representation to return, hence 204.
    /// </remarks>
    /// <response code="204">Email confirmed successfully.</response>
    /// <response code="400">
    /// The code is invalid, expired, already used, or the max attempt count was
    /// exceeded (which also discards the code, requiring a new one to be requested);
    /// or the email was already verified.
    /// </response>
    /// <response code="404">No user exists with the specified email.</response>
    [HttpPut]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmEmailAsync(
        [FromBody] VerifyEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        await _emailVerificationService.VerifyEmailAsync(request, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Issues a new verification code and emails it to the user
    /// </summary>
    /// <param name="request">The email to receive the verification code.</param>
    /// <param name="cancellationToken"></param>
    /// <response code="202">A new verification code was generated and emailed.</response>
    /// <response code="400">The email is already verified.</response>
    /// <response code="404">No user exists with the specified email.</response>
    [HttpPost("codes")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateVerificationCodeAsync(
        [FromBody] SendVerificationCodeRequest request,
        CancellationToken cancellationToken = default)
    {
        await _emailVerificationService.SendVerificationCodeAsync(request, cancellationToken);

        return Accepted();
    }
}