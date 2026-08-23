using FluentValidation;

namespace GEC.ApplicationCore.DTOs.Categories;

public class CheckSlugAvailabilityRequestValidator : AbstractValidator<CheckSlugAvailabilityRequest>
{
    public CheckSlugAvailabilityRequestValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(100)
            .Matches(@"^[a-z0-9]+(-[a-z0-9]+)*$")
            .WithMessage("Slug must be lowercase letters, numbers, and hyphens only (e.g. 'home-and-kitchen').");

        RuleFor(x => x.ExcludeCategoryId)
            .NotEqual(Guid.Empty)
            .WithMessage("ExcludeCategoryId cannot be an empty GUID.")
            .When(x => x.ExcludeCategoryId.HasValue);
    }
}