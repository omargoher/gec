using System.Security.Claims;
using GEC.ApplicationCore.DTOs.Auth;
using GEC.ApplicationCore.Exceptions;
using GEC.ApplicationCore.Interfaces.Services;
using GEC.ApplicationCore.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace GEC.API.Controllers;

/// <summary>
/// Registration and session (login / refresh / logout) endpoints.
/// </summary>
/// <remarks>
/// The access token is returned to the client as an HTTP-Only cookie, not in the
/// response body — the client never reads or handles it directly, it's just sent
/// back automatically by the browser on subsequent requests. The refresh token is
/// likewise never exposed to client-side script: it's set as an httpOnly, secure
/// cookie and rotated every time it's used.
/// </remarks>
[ApiController]
[Route("api/auth")]
[Tags("Auth")]
public class AuthController : ControllerBase
{
    private const string AccessTokenCookieName = "accessToken";
    private const string RefreshTokenCookieName = "refreshToken";

    private readonly IAuthenticationService _authenticationService;
    private readonly JwtOptions _jwtOptions;

    public AuthController(
        IAuthenticationService authenticationService,
        IOptions<JwtOptions> jwtOptions)
    {
        _authenticationService = authenticationService;
        _jwtOptions = jwtOptions.Value;
    }

    /// <summary>
    /// Creates a new account (a "registration").
    /// </summary>
    /// <param name="request">Email, password, and display name for the new account.</param>
    /// <param name="cancellationToken"></param>
    /// <remarks>
    /// The account is created and assigned the default "Customer" role even if the
    /// verification email fails to send — email delivery is best-effort and is not
    /// rolled back on failure, so a 200 here does not guarantee the user received
    /// an OTP. The account still requires email confirmation before
    /// </remarks>
    /// <response code="200">Account created; AppUserDto details returned.</response>
    /// <response code="400">Validation failed, or the password didn't meet the identity policy.</response>
    /// <response code="409">An account with this email already exists.</response>
    [HttpPost("registrations")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AppUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AppUserDto>> CreateRegistrationAsync(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authenticationService.RegisterAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a session (logs in) with an email and password.
    /// </summary>
    /// <param name="request">Email and password.</param>
    /// <param name="cancellationToken"></param>
    /// <response code="204">Session created; access and refresh tokens set as HTTP-Only cookies.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">
    /// Invalid credentials, the account is locked out, or the account's email has not
    /// been confirmed yet. All three cases return the same generic message to avoid
    /// leaking account existence or lockout state to the caller.
    /// </response>
    [HttpPost("sessions")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateSessionAsync(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authenticationService.LoginAsync(request, cancellationToken);

        SetAccessTokenCookie(result.AccessToken);
        SetRefreshTokenCookie(result.RefreshToken);

        return NoContent();
    }

    /// <summary>
    /// Replaces the current session's tokens (refreshes) using the refresh token cookie.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <remarks>
    /// This endpoint allows anonymous requests so clients with expired access tokens 
    /// can obtain a new token pair using a valid, unexpired refresh token cookie.
    /// </remarks>
    /// <response code="204">Tokens rotated successfully; new access and refresh cookies set.</response>
    /// <response code="401">
    /// Missing, expired, or invalid refresh token cookie.
    /// </response>
    [HttpPut("sessions/current")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshSessionAsync(
        CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(RefreshTokenCookieName, out var rawRefreshToken) ||
            string.IsNullOrWhiteSpace(rawRefreshToken))
        {
            throw new UnauthorizedException("You are not authenticated.");
        }

        var result = await _authenticationService.RefreshAsync(rawRefreshToken, cancellationToken);

        SetAccessTokenCookie(result.AccessToken);
        SetRefreshTokenCookie(result.RefreshToken);

        return NoContent();
    }

    /// <summary>
    /// Revokes the current session (logs out) and clears both authentication cookies.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <remarks>
    /// Always returns 204, even if the refresh token cookie was missing or already
    /// revoked, so the client can treat this as "now logged out" either way.
    /// </remarks>
    /// <response code="204">Session revoked and cookies cleared.</response>
    /// <response code="401">Missing or invalid access token.</response>
    [HttpDelete("sessions/current")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RevokeSessionAsync(CancellationToken cancellationToken)
    {
        if (Request.Cookies.TryGetValue(RefreshTokenCookieName, out var rawRefreshToken) &&
            !string.IsNullOrWhiteSpace(rawRefreshToken))
        {
            await _authenticationService.LogoutAsync(rawRefreshToken, cancellationToken);
        }

        DeleteAuthCookies();

        return NoContent();
    }

    /// <summary>
    /// Revokes every active session (refresh token) for the current user, ending all
    /// sessions on every device, and clears this device's cookies.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <remarks>
    /// Requires a valid access token. Cookies are only cleared on this device;
    /// other devices are not notified and will simply fail their next refresh attempt.
    /// </remarks>
    /// <response code="204">All sessions revoked.</response>
    /// <response code="401">Missing or invalid access token.</response>
    [HttpDelete("sessions")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RevokeAllSessionsAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedException("You are not authenticated.");
        }

        await _authenticationService.LogoutAllAsync(userId, cancellationToken);

        DeleteAuthCookies();

        return NoContent();
    }

    private void SetAccessTokenCookie(string accessToken)
    {
        Response.Cookies.Append(AccessTokenCookieName, accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes)
        });
    }

    private void SetRefreshTokenCookie(string rawRefreshToken)
    {
        Response.Cookies.Append(RefreshTokenCookieName, rawRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshTokenDays)
        });
    }

    private void DeleteAuthCookies()
    {
        Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            Path = "/"
        });

        Response.Cookies.Delete(AccessTokenCookieName, new CookieOptions
        {
            Path = "/"
        });
    }
}