using GEC.Domain.Enums;

namespace GEC.Domain.Entities;

public class Address : BaseEntity
{
    public Guid CustomerId { get; set; }

    public Customer Customer { get; set; } = default!;

    public AddressLabel Label { get; set; } = AddressLabel.Home;

    public string ReceiverName { get; set; } = default!;

    public string PhoneNumber { get; set; } = default!;

    public string Country { get; set; } = "Egypt";

    public string Governorate { get; set; } = default!;

    public string City { get; set; } = default!;

    public string District { get; set; } = default!;

    public string Street { get; set; } = default!;

    public string Building { get; set; } = default!;

    public string Apartment { get; set; } = default!;

    public string? Landmark { get; set; }

    public string? Notes { get; set; }

    public string? PostalCode { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public bool IsDefault { get; set; }
}