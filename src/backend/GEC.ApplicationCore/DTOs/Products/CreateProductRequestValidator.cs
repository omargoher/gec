using FluentValidation;
namespace GEC.ApplicationCore.DTOs.Products;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(255)
            .Matches(@"^[a-z0-9]+(-[a-z0-9]+)*$")
            .WithMessage("Slug must be lowercase letters, numbers, and hyphens only (e.g. 'pro-running-shoes').");

        RuleFor(x => x.BaseDescription)
            .MaximumLength(5000);
    }
}
