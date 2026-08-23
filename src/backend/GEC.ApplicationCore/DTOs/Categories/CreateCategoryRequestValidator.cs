using FluentValidation;

namespace GEC.ApplicationCore.DTOs.Categories;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(100)
            .Matches(@"^[a-z0-9]+(-[a-z0-9]+)*$")
            .WithMessage("Slug must be lowercase letters, numbers, and hyphens only (e.g. 'home-and-kitchen').");

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.Icon)
            .MaximumLength(100);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(2048)
            .Must(BeAValidUrl)
            .WithMessage("ImageUrl must be a valid absolute URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));

        RuleFor(x => x.ParentId)
            .NotEqual(Guid.Empty)
            .WithMessage("ParentId cannot be an empty GUID.")
            .When(x => x.ParentId.HasValue);
    }

    private static bool BeAValidUrl(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out _);
}