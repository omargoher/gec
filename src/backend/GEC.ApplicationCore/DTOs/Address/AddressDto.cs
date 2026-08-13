using GEC.Domain.Enums;

namespace GEC.ApplicationCore.DTOs;

public record AddressDto(
    Guid Id,
    AddressLabel Label,
    string ReceiverName,
    string PhoneNumber,
    string Country,
    string Governorate,
    string City,
    string District,
    string Street,
    string Building,
    string Apartment,
    string? Landmark,
    string? Notes,
    string? PostalCode,
    double? Latitude,
    double? Longitude,
    bool IsDefault);