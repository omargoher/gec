using GEC.Domain.Enums;

namespace GEC.ApplicationCore.DTOs;

public record AddressRequest(
    AddressLabel Label,
    string ReceiverFirstName,
    string ReceiverLastName,
    string PhoneNumber,
    string Governorate,
    string City,
    string District,
    string Street,
    string? Building,
    string? Apartment,
    string? Landmark,
    string? Notes,
    string? PostalCode,
    double? Latitude,
    double? Longitude);