using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GEC.API.Controllers;

/// <summary>
/// plaplplapl
/// </summary>
/// <remarks>
/// plaplpalpla
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Tags("test")]
public class TestController : ControllerBase
{

    public TestController()
    {
    }

    /// <summary>
    /// test endpoint.
    /// </summary>
    /// <remarks>
    /// some text
    /// some text
    /// some text
    /// </remarks>
    /// <param name="test">test string.</param>
    /// <response code="200">may.</response>
    /// <response code="400">Invalid request payload.</response>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> TestAsync([FromBody] string test)
    {
        return Ok(test);
    }
}