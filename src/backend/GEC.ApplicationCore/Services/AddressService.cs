using GEC.ApplicationCore.DTOs;
using GEC.ApplicationCore.Exceptions;
using GEC.ApplicationCore.Interfaces.Persistence;
using GEC.ApplicationCore.Interfaces.Repositories;
using GEC.ApplicationCore.Interfaces.Services;
using GEC.Domain.Entities;

namespace GEC.ApplicationCore.Services;

public class AddressService : IAddressService
{
    private readonly IAddressRepository _addressRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddressService(IAddressRepository addressRepository, IUnitOfWork unitOfWork)
    {
        _addressRepository = addressRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task AddAddressAsync(
        Guid customerId,
        AddressRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // A customer's very first address becomes their default automatically.
        // Every subsequent address is non-default until they explicitly call
        // SetDefaultAddressAsync — that's the only path allowed to flip IsDefault,
        // which is what the unique filtered index on (CustomerId, IsDefault) relies on.
        var hasExistingAddress = await _addressRepository.HasAnyAddressAsync(
            customerId,
            cancellationToken);

        var address = new Address
        {
            CustomerId = customerId,
            Label = request.Label,
            ReceiverName = request.ReceiverFirstName,
            PhoneNumber = request.PhoneNumber,
            Governorate = request.Governorate,
            City = request.City,
            District = request.District,
            Street = request.Street,
            Building = request.Building,
            Apartment = request.Apartment,
            Landmark = request.Landmark,
            Notes = request.Notes,
            PostalCode = request.PostalCode,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IsDefault = !hasExistingAddress
        };

        _addressRepository.Add(address);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAddressAsync(
        Guid customerId,
        Guid addressId,
        AddressRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var address = await _addressRepository.GetByIdAndCustomerIdAsync(
            addressId,
            customerId,
            cancellationToken);

        // Deliberately NotFound rather than Forbidden here: the query already
        // filters by customerId, so "exists but belongs to someone else" and
        // "doesn't exist" are indistinguishable — and should stay that way,
        // so a caller can't use a 403 vs 404 split to enumerate other
        // customers' address IDs.
        if (address is null)
            throw new NotFoundException("Address");

        address.Label = request.Label;
        address.ReceiverName = request.ReceiverFirstName;
        address.PhoneNumber = request.PhoneNumber;
        address.Governorate = request.Governorate;
        address.City = request.City;
        address.District = request.District;
        address.Street = request.Street;
        address.Building = request.Building;
        address.Apartment = request.Apartment;
        address.Landmark = request.Landmark;
        address.Notes = request.Notes;
        address.PostalCode = request.PostalCode;
        address.Latitude = request.Latitude;
        address.Longitude = request.Longitude;
        // IsDefault is intentionally not touched here — see SetDefaultAddressAsync.

        _addressRepository.Update(address);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAddressAsync(
        Guid customerId,
        Guid addressId,
        CancellationToken cancellationToken = default)
    {
        var address = await _addressRepository.GetByIdAndCustomerIdAsync(
            addressId,
            customerId,
            cancellationToken);

        if (address is null)
            throw new NotFoundException("Address");

        var wasDefault = address.IsDefault;

        _addressRepository.Remove(address);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!wasDefault)
            return;

        // The deleted address was the default — promote the customer's next
        // most-recent remaining address so they aren't left with zero.
        var remaining = await _addressRepository.GetByCustomerIdAsync(
            customerId,
            cancellationToken);

        var nextDefault = remaining.FirstOrDefault();

        if (nextDefault is not null)
        {
            await _addressRepository.SetDefaultAsync(
                customerId,
                nextDefault.Id,
                cancellationToken);
        }
    }

    public async Task<AddressDto> GetAddressByIdAsync(
        Guid customerId,
        Guid addressId,
        CancellationToken cancellationToken = default)
    {
        var address = await _addressRepository.GetByIdAndCustomerIdAsync(
            addressId,
            customerId,
            cancellationToken);

        if (address is null)
            throw new NotFoundException("Address");

        return MapToDto(address);
    }

    public async Task<IReadOnlyList<AddressDto>> GetCustomerAddressesAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var addresses = await _addressRepository.GetByCustomerIdAsync(
            customerId,
            cancellationToken);

        return addresses
            .Select(MapToDto)
            .ToList();
    }

    public async Task SetDefaultAddressAsync(
        Guid customerId,
        Guid addressId,
        CancellationToken cancellationToken = default)
    {
        // SetDefaultAsync already validates ownership/existence (NotFoundException)
        // and performs the unset-old/set-new swap transactionally.
        await _addressRepository.SetDefaultAsync(
            customerId,
            addressId,
            cancellationToken);
    }

    private static AddressDto MapToDto(Address address)
    {
        return new AddressDto(
            address.Id,
            address.Label,
            address.ReceiverName,
            address.PhoneNumber,
            address.Country,
            address.Governorate,
            address.City,
            address.District,
            address.Street,
            address.Building,
            address.Apartment,
            address.Landmark,
            address.Notes,
            address.PostalCode,
            address.Latitude,
            address.Longitude,
            address.IsDefault);
    }
}