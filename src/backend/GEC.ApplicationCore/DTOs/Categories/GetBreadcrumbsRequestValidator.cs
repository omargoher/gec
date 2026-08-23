using FluentValidation;

namespace GEC.ApplicationCore.DTOs.Categories;

public class GetBreadcrumbsRequestValidator : AbstractValidator<GetBreadcrumbsRequest>
{
    public GetBreadcrumbsRequestValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(100)
            .Matches(@"^[a-z0-9]+(-[a-z0-9]+)*$")
            .WithMessage("Slug must be lowercase letters, numbers, and hyphens only (e.g. 'home-and-kitchen').");
    }
}