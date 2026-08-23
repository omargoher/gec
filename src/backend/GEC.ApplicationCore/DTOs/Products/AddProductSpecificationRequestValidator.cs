using FluentValidation;
namespace GEC.ApplicationCore.DTOs.Products;

public class AddProductSpecificationRequestValidator : AbstractValidator<AddProductSpecificationRequest>
{
    public AddProductSpecificationRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Value)
            .NotEmpty()
            .MaximumLength(500);
    }
}
