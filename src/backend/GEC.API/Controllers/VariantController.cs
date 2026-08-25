using GEC.ApplicationCore.DTOs.Products;
using GEC.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GEC.API.Controllers;

/// <summary>
/// Variant shortcuts: look up a variant directly by ID or SKU without knowing its product.
/// </summary>
[ApiController]
[Route("api/variants")]
[Tags("Variants")]
public class VariantController : ControllerBase
{
    private readonly IProductService _productService;

    public VariantController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>Returns a variant by its ID (includes RowVersion and attribute values).</summary>
    /// <response code="200">Variant retrieved.</response>
    /// <response code="404">Variant not found.</response>
    [AllowAnonymous]
    [HttpGet("{variantId:guid}", Name = "GetVariantById")]
    [ProducesResponseType(typeof(VariantDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VariantDetailsResponse>> GetByIdAsync(
        Guid variantId,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetVariantByIdAsync(variantId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Returns a variant by its SKU (case-insensitive).</summary>
    /// <response code="200">Variant retrieved.</response>
    /// <response code="404">Variant not found.</response>
    [AllowAnonymous]
    [HttpGet("sku/{sku}")]
    [ProducesResponseType(typeof(VariantDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VariantDetailsResponse>> GetBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetVariantBySkuAsync(sku, cancellationToken);
        return Ok(result);
    }
}
