using FluentValidation;

namespace GEC.ApplicationCore.DTOs;

public class AddressRequestValidator : AbstractValidator<AddressRequest>
{
    public AddressRequestValidator()
    {
        RuleFor(x => x.Label).IsInEnum();

        RuleFor(x => x.ReceiverFirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ReceiverLastName).NotEmpty().MaximumLength(100);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .MaximumLength(20)
            .Matches(@"^\+?[0-9]{7,15}$")
            .WithMessage("Phone number must contain 7-15 digits, with an optional leading '+'.");

        RuleFor(x => x.Governorate).NotEmpty().MaximumLength(100);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.District).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Street).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Building).MaximumLength(50);
        RuleFor(x => x.Apartment).MaximumLength(50);
        RuleFor(x => x.Landmark).MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(500);
        RuleFor(x => x.PostalCode).MaximumLength(20);

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .When(x => x.Latitude is not null);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .When(x => x.Longitude is not null);
    }
}