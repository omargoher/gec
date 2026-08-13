using GEC.ApplicationCore.DTOs;

namespace GEC.ApplicationCore.Interfaces.Services;

public interface IAddressService
{
    Task AddAddressAsync(
        Guid customerId,
        AddressRequest request,
        CancellationToken cancellationToken = default);

    Task UpdateAddressAsync(
        Guid customerId,
        Guid AddressId,
        AddressRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAddressAsync(
        Guid customerId,
        Guid addressId,
        CancellationToken cancellationToken = default);

    Task<AddressDto> GetAddressByIdAsync(
        Guid customerId,
        Guid addressId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AddressDto>> GetCustomerAddressesAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task SetDefaultAddressAsync(
        Guid customerId,
        Guid addressId,
        CancellationToken cancellationToken = default);
}