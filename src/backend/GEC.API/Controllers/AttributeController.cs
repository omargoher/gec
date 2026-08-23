using GEC.ApplicationCore.DTOs;
using GEC.ApplicationCore.DTOs.Attributes;
using GEC.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GEC.API.Controllers;

/// <summary>
/// Global attribute definition and value management.
/// </summary>
[ApiController]
[Route("api/attributes")]
[Tags("Attributes")]
public class AttributeController : ControllerBase
{
    private readonly IAttributeService _attributeService;

    public AttributeController(IAttributeService attributeService)
    {
        _attributeService = attributeService;
    }

    /// <summary>Creates a new global attribute definition (e.g. Color, Size).</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(AttributeDefinitionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AttributeDefinitionResponse>> CreateAsync(
        [FromBody] CreateAttributeDefinitionRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _attributeService.AddAttributeDefinitionAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Deletes an attribute definition. Fails if it is attached to any product or has values.</summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{attributeId:guid}")]
    [ProducesResponseType(typeof(AttributeDefinitionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AttributeDefinitionResponse>> DeleteAsync(
        Guid attributeId,
        CancellationToken cancellationToken = default)
    {
        var result = await _attributeService.RemoveAttributeDefinitionAsync(attributeId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Returns a paged list of attribute definitions.</summary>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AttributeDefinitionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AttributeDefinitionResponse>>> GetAllAsync(
        [FromQuery] GetAttributeDefinitionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _attributeService.GetAttributeDefinitionsAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Adds a value to an attribute (e.g. Red, Blue to Color).</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{attributeId:guid}/values")]
    [ProducesResponseType(typeof(AttributeValueResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AttributeValueResponse>> AddValueAsync(
        Guid attributeId,
        [FromBody] CreateAttributeValueRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _attributeService.AddAttributeValueAsync(attributeId, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Removes an attribute value. Fails if any variant is still using it.</summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{attributeId:guid}/values/{valueId:guid}")]
    [ProducesResponseType(typeof(AttributeValueResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AttributeValueResponse>> DeleteValueAsync(
        Guid attributeId,
        Guid valueId,
        CancellationToken cancellationToken = default)
    {
        var result = await _attributeService.RemoveAttributeValueAsync(attributeId, valueId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Returns all values for an attribute definition.</summary>
    [AllowAnonymous]
    [HttpGet("{attributeId:guid}/values")]
    [ProducesResponseType(typeof(List<AttributeValueResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<AttributeValueResponse>>> GetValuesAsync(
        Guid attributeId,
        CancellationToken cancellationToken = default)
    {
        var result = await _attributeService.GetAttributeValuesByAttributeIdAsync(attributeId, cancellationToken);
        return Ok(result);
    }
}
