using GEC.ApplicationCore.DTOs;
using GEC.ApplicationCore.Interfaces.Identity;
using GEC.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GEC.API.Controllers;

/// <summary>
/// Handles the authenticated customer's shipping addresses.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Addresses")]
[Authorize]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;
    private readonly ICurrentUserService _currentUserService;

    public AddressController(IAddressService addressService, ICurrentUserService currentUserService)
    {
        _addressService = addressService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Gets all addresses belonging to the current customer.
    /// </summary>
    /// <response code="200">Addresses retrieved successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AddressDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AddressDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var customerId = await _currentUserService.GetCustomerIdAsync(cancellationToken);
        var addresses = await _addressService.GetCustomerAddressesAsync(customerId, cancellationToken);
        return Ok(addresses);
    }

    /// <summary>
    /// Gets a single address by id.
    /// </summary>
    /// <response code="200">Address retrieved successfully.</response>
    /// <response code="404">Address was not found.</response>
    [HttpGet("{addressId:guid}")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AddressDto>> GetByIdAsync(Guid addressId, CancellationToken cancellationToken = default)
    {
        var customerId = await _currentUserService.GetCustomerIdAsync(cancellationToken);
        var address = await _addressService.GetAddressByIdAsync(customerId, addressId, cancellationToken);
        return Ok(address);
    }

    /// <summary>
    /// Adds a new address for the current customer.
    /// </summary>
    /// <response code="201">Address created successfully.</response>
    /// <response code="400">Invalid request.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddAsync([FromBody] AddressRequest request, CancellationToken cancellationToken = default)
    {
        var customerId = await _currentUserService.GetCustomerIdAsync(cancellationToken);
        await _addressService.AddAddressAsync(customerId, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created);
    }

    /// <summary>
    /// Updates an existing address.
    /// </summary>
    /// <response code="204">Address updated successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="404">Address was not found.</response>
    [HttpPut("{addressId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(Guid addressId, [FromBody] AddressRequest request, CancellationToken cancellationToken = default)
    {
        var customerId = await _currentUserService.GetCustomerIdAsync(cancellationToken);
        await _addressService.UpdateAddressAsync(customerId, addressId, request, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes an address.
    /// </summary>
    /// <response code="204">Address deleted successfully.</response>
    /// <response code="404">Address was not found.</response>
    [HttpDelete("{addressId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid addressId, CancellationToken cancellationToken = default)
    {
        var customerId = await _currentUserService.GetCustomerIdAsync(cancellationToken);
        await _addressService.DeleteAddressAsync(customerId, addressId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sets an address as the customer's default.
    /// </summary>
    /// <response code="204">Default address set successfully.</response>
    /// <response code="404">Address was not found.</response>
    [HttpPost("{addressId:guid}/default")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetDefaultAsync(Guid addressId, CancellationToken cancellationToken = default)
    {
        var customerId = await _currentUserService.GetCustomerIdAsync(cancellationToken);
        await _addressService.SetDefaultAddressAsync(customerId, addressId, cancellationToken);
        return NoContent();
    }
}