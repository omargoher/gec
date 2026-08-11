using FluentValidation;

namespace GEC.ApplicationCore.DTOs.Profile;

public class ProfileRequestValidator : AbstractValidator<ProfileRequest>
{
    public ProfileRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
    }
}