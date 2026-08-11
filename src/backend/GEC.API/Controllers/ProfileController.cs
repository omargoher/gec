using GEC.ApplicationCore.DTOs.Profile;
using GEC.ApplicationCore.Interfaces.Identity;
using GEC.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GEC.API.Controllers;

/// <summary>
/// Handles the authenticated customer's profile management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly ICurrentUserService _currentUserService;

    public ProfileController(IProfileService profileService, ICurrentUserService currentUserService)
    {
        _profileService = profileService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Gets the current authenticated customer's profile.
    /// </summary>
    /// <response code="200">Profile retrieved successfully.</response>
    /// <response code="404">Customer was not found.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfileDto>> GetAsync(CancellationToken cancellationToken = default)
    {
        var profile = await _profileService.GetProfileAsync(await _currentUserService.GetCustomerIdAsync(cancellationToken), cancellationToken);
        return Ok(profile);
    }

    /// <summary>
    /// Updates the current authenticated customer's profile.
    /// </summary>
    /// <response code="204">Profile updated successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="404">Customer was not found.</response>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync([FromBody] ProfileRequest request, CancellationToken cancellationToken = default)
    {
        await _profileService.UpdateProfileAsync(await _currentUserService.GetCustomerIdAsync(cancellationToken), request, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Changes the current authenticated customer's password.
    /// </summary>
    /// <response code="204">Password changed successfully.</response>
    /// <response code="400">Invalid current password or request.</response>
    /// <response code="404">Customer was not found.</response>
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        await _profileService.ChangePasswordAsync(_currentUserService.UserId!, request, cancellationToken);

        Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            Path = "/"
        });

        Response.Cookies.Delete("accessToken", new CookieOptions
        {
            Path = "/"
        });

        return NoContent();
    }
}