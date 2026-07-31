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
/// Registration, login, refresh, and logout endpoints.
/// </summary>
/// <remarks>
/// The access token is returned in the response body and must be sent as a
/// <c>Bearer</c> token on subsequent requests. The refresh token is never
/// exposed to the client-side script: it's set as an httpOnly, secure cookie
/// scoped to <c>/api/auth</c>, and rotated on every refresh.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
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
    /// Registers a new account.
    /// </summary>
    /// <param name="request">Email, password, and display name for the new account.</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Account created; AppUserDto details returned.</response>
    /// <response code="400">Validation failed, or the password didn't meet the identity policy.</response>
    /// <response code="409">An account with this email already exists.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AppUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AppUserDto>> RegisterAsync(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authenticationService.RegisterAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Logs in with an email and password.
    /// </summary>
    /// <param name="request">Email and password.</param>
    /// <param name="cancellationToken"></param>
    /// <response code="204">Logged in successfully, access and refresh tokens set as HTTP-Only cookies.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Invalid credentials, or the account is locked out.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginAsync(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authenticationService.LoginAsync(request, cancellationToken);

        SetAccessTokenCookie(result.AccessToken);
        SetRefreshTokenCookie(result.RefreshToken);

        return NoContent();
    }

    /// <summary>
    /// Exchanges the refresh token cookie for a new access token, rotating both tokens in HTTP-Only cookies.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <response code="204">Tokens rotated successfully.</response>
    /// <response code="401">Missing, expired, or already-used refresh token. The client should redirect to login.</response>
    [HttpPost("refresh")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshAsync(
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
    /// Revokes the current refresh token and clears both authentication cookies.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <remarks>
    /// Always returns 204, even if there was no valid refresh token to revoke,
    /// so the client can treat logout as "now logged out" either way.
    /// </remarks>
    /// <response code="204">Logged out.</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken)
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
    /// Revokes every active refresh token for the current user, ending all
    /// sessions on every device and clearing cookies.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <remarks>
    /// Requires a valid access token. Only clears this device's cookies
    /// directly; other devices simply fail their next refresh attempt.
    /// </remarks>
    /// <response code="204">All sessions revoked.</response>
    /// <response code="401">Missing or invalid access token.</response>
    [HttpPost("logout-all")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogoutAllAsync(CancellationToken cancellationToken)
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