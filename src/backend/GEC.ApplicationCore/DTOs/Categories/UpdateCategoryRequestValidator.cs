using FluentValidation;

namespace GEC.ApplicationCore.DTOs.Categories;

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        // All fields are optional on update (nullable = "not sent"), so each rule
        // only fires when the client actually included that field.

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .When(x => x.Name is not null);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(100)
            .Matches(@"^[a-z0-9]+(-[a-z0-9]+)*$")
            .WithMessage("Slug must be lowercase letters, numbers, and hyphens only (e.g. 'home-and-kitchen').")
            .When(x => x.Slug is not null);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => x.Description is not null);

        RuleFor(x => x.Icon)
            .MaximumLength(100)
            .When(x => x.Icon is not null);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(2048)
            .Must(BeAValidUrl)
            .WithMessage("ImageUrl must be a valid absolute URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));

        RuleFor(x => x.ParentId)
            .NotEqual(Guid.Empty)
            .WithMessage("ParentId cannot be an empty GUID.")
            .When(x => x.ParentId.HasValue);

        RuleFor(x => x)
            .Must(x => !(x.ClearParent == true && x.ParentId.HasValue))
            .WithMessage("Cannot set ParentId and ClearParent at the same time.")
            .WithName("ParentId");
    }

    private static bool BeAValidUrl(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out _);
}