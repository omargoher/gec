mkdir -p /home/omar/RiderProjects/gec/src/backend/GEC.API/Controllers/

cat << 'EOF' > /home/omar/RiderProjects/gec/src/backend/GEC.API/Controllers/ProductController.cs
using GEC.ApplicationCore.DTOs;
using GEC.ApplicationCore.DTOs.Products;
using GEC.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GEC.API.Controllers;

/// <summary>
/// Product catalog management (admin) and browsing (public).
/// </summary>
[ApiController]
[Route("api/products")]
[Tags("Products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>Creates a new product in Draft status.</summary>
    /// <response code="201">Product created.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="409">Slug already in use.</response>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponse>> CreateAsync(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await _productService.CreateProductAsync(request, cancellationToken);
        return CreatedAtRoute("GetProductById", new { productId = product.Id }, product);
    }

    /// <summary>Returns a paged list of products. Supports optional Status filter and keyword search.</summary>
    /// <response code="200">Products retrieved.</response>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductListItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductListItemResponse>>> GetAllAsync(
        [FromQuery] GetProductsRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetAllProductsAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Returns the full details of a product by ID.</summary>
    /// <response code="200">Product retrieved.</response>
    /// <response code="404">Product not found.</response>
    [AllowAnonymous]
    [HttpGet("{productId:guid}", Name = "GetProductById")]
    [ProducesResponseType(typeof(ProductDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDetailsResponse>> GetByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var product = await _productService.GetProductByIdAsync(productId, cancellationToken);
        return Ok(product);
    }

    /// <summary>Changes a product's lifecycle status. Requires RowVersion.</summary>
    /// <response code="200">Status changed.</response>
    /// <response code="400">Invalid transition or business rule violation.</response>
    /// <response code="404">Product not found.</response>
    /// <response code="409">Concurrency conflict or status conflict.</response>
    [Authorize(Roles = "Admin")]
    [HttpPatch("{productId:guid}/status")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponse>> ChangeStatusAsync(
        Guid productId,
        [FromBody] ChangeProductStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await _productService.ChangeProductStatusAsync(productId, request, cancellationToken);
        return Ok(product);
    }

    /// <summary>Archives a product. Requires RowVersion. Terminal operation.</summary>
    /// <response code="200">Product archived.</response>
    /// <response code="400">Already archived or invalid request.</response>
    /// <response code="404">Product not found.</response>
    /// <response code="409">Concurrency conflict.</response>
    [Authorize(Roles = "Admin")]
    [HttpPost("{productId:guid}/archive")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponse>> ArchiveAsync(
        Guid productId,
        [FromBody] ArchiveProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await _productService.ArchiveProductAsync(productId, request, cancellationToken);
        return Ok(product);
    }

    // ─── Product Attributes ────────────────────────────────────────────

    /// <summary>Returns all attributes enabled for a product.</summary>
    [AllowAnonymous]
    [HttpGet("{productId:guid}/attributes")]
    [ProducesResponseType(typeof(List<ProductAttributeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ProductAttributeResponse>>> GetAttributesAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetProductAttributesByProductIdAsync(productId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Enables an attribute for a product (marks it required or optional).</summary>
    /// <response code="201">Attribute enabled.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">Product or attribute not found.</response>
    /// <response code="409">Attribute already enabled for this product.</response>
    [Authorize(Roles = "Admin")]
    [HttpPost("{productId:guid}/attributes")]
    [ProducesResponseType(typeof(ProductAttributeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductAttributeResponse>> AddAttributeAsync(
        Guid productId,
        [FromBody] AddProductAttributeRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.AddProductAttributeAsync(productId, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Removes an attribute from a product. Fails if any variant still uses the attribute.
    /// </summary>
    /// <response code="204">Attribute removed.</response>
    /// <response code="400">Attribute still in use by variants.</response>
    /// <response code="404">Product or attribute not found.</response>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{productId:guid}/attributes/{attributeId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveAttributeAsync(
        Guid productId,
        Guid attributeId,
        CancellationToken cancellationToken = default)
    {
        await _productService.RemoveProductAttributeAsync(productId, attributeId, cancellationToken);
        return NoContent();
    }

    // ─── Product Specifications ───────────────────────────────────────

    /// <summary>Appends a key/value specification to a product (add-only).</summary>
    /// <response code="201">Specification added.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">Product not found.</response>
    [Authorize(Roles = "Admin")]
    [HttpPost("{productId:guid}/specifications")]
    [ProducesResponseType(typeof(ProductSpecificationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductSpecificationResponse>> AddSpecificationAsync(
        Guid productId,
        [FromBody] AddProductSpecificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.AddProductSpecificationAsync(productId, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    // ─── Variants ─────────────────────────────────────────────────────────

    /// <summary>Returns all variants of a product (list view, no RowVersion).</summary>
    [AllowAnonymous]
    [HttpGet("{productId:guid}/variants")]
    [ProducesResponseType(typeof(List<VariantListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<VariantListItemResponse>>> GetVariantsAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetAllVariantsByProductIdAsync(productId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Adds a new variant (SKU) to the product in Draft status.</summary>
    /// <response code="201">Variant created.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">Product not found.</response>
    /// <response code="409">SKU already in use.</response>
    [Authorize(Roles = "Admin")]
    [HttpPost("{productId:guid}/variants")]
    [ProducesResponseType(typeof(VariantResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VariantResponse>> AddVariantAsync(
        Guid productId,
        [FromBody] CreateVariantRequest request,
        CancellationToken cancellationToken = default)
    {
        var variant = await _productService.AddVariantAsync(productId, request, cancellationToken);
        return CreatedAtRoute("GetVariantById", new { variantId = variant.Id }, variant);
    }

    /// <summary>Returns full details of a variant (including RowVersion).</summary>
    [AllowAnonymous]
    [HttpGet("{productId:guid}/variants/{variantId:guid}")]
    [ProducesResponseType(typeof(VariantDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VariantDetailsResponse>> GetVariantByIdAsync(
        Guid productId,
        Guid variantId,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetVariantByIdAsync(variantId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Removes a variant. Prefer status changes for normal flows.</summary>
    /// <response code="200">Variant removed.</response>
    /// <response code="404">Product or variant not found.</response>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{productId:guid}/variants/{variantId:guid}")]
    [ProducesResponseType(typeof(VariantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VariantResponse>> RemoveVariantAsync(
        Guid productId,
        Guid variantId,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.RemoveVariantAsync(productId, variantId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Updates variant SKU, price, and/or currency. Requires RowVersion.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPatch("{productId:guid}/variants/{variantId:guid}")]
    [ProducesResponseType(typeof(VariantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VariantResponse>> UpdateVariantAsync(
        Guid productId,
        Guid variantId,
        [FromBody] UpdateVariantRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.UpdateVariantAsync(productId, variantId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Updates variant price and currency. Requires RowVersion.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPatch("{productId:guid}/variants/{variantId:guid}/price")]
    [ProducesResponseType(typeof(VariantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VariantResponse>> UpdateVariantPriceAsync(
        Guid productId,
        Guid variantId,
        [FromBody] UpdateVariantPriceRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.UpdateVariantPriceAsync(productId, variantId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Changes variant lifecycle status. Requires RowVersion.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPatch("{productId:guid}/variants/{variantId:guid}/status")]
    [ProducesResponseType(typeof(VariantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VariantResponse>> ChangeVariantStatusAsync(
        Guid productId,
        Guid variantId,
        [FromBody] ChangeVariantStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.ChangeVariantStatusAsync(variantId, request, cancellationToken);
        return Ok(result);
    }

    // ─── Variant Attribute Values ────────────────────────────────────

    /// <summary>Assigns an attribute value to a variant (for an attribute not yet set). Requires RowVersion.</summary>
    /// <response code="200">Value assigned, VariantSignature updated.</response>
    /// <response code="400">Validation failed or business rule violation.</response>
    /// <response code="404">Product, variant, or attribute value not found.</response>
    /// <response code="409">Duplicate combination or concurrency conflict.</response>
    [Authorize(Roles = "Admin")]
    [HttpPost("{productId:guid}/variants/{variantId:guid}/attribute-values")]
    [ProducesResponseType(typeof(VariantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VariantResponse>> AssignAttributeValueAsync(
        Guid productId,
        Guid variantId,
        [FromBody] AssignVariantAttributeValueRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.AssignVariantAttributeValueAsync(productId, variantId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Reassigns a different value for an already-set attribute on a variant. Requires RowVersion.</summary>
    /// <response code="200">Value updated, VariantSignature recomputed.</response>
    [Authorize(Roles = "Admin")]
    [HttpPut("{productId:guid}/variants/{variantId:guid}/attribute-values/{attributeId:guid}")]
    [ProducesResponseType(typeof(VariantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VariantResponse>> ReassignAttributeValueAsync(
        Guid productId,
        Guid variantId,
        Guid attributeId,
        [FromBody] ReassignVariantAttributeValueRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.ReassignVariantAttributeValueAsync(productId, variantId, request, cancellationToken);
        return Ok(result);
    }
}
EOF

cat << 'EOF' > /home/omar/RiderProjects/gec/src/backend/GEC.API/Controllers/AttributeController.cs
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
EOF

cat << 'EOF' > /home/omar/RiderProjects/gec/src/backend/GEC.API/Controllers/VariantController.cs
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
