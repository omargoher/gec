using FluentValidation;
namespace GEC.ApplicationCore.DTOs.Products;

public class AddProductAttributeRequestValidator : AbstractValidator<AddProductAttributeRequest>
{
    public AddProductAttributeRequestValidator()
    {
        RuleFor(x => x.AttributeId)
            .NotEqual(Guid.Empty)
            .WithMessage("AttributeId must not be empty.");
    }
}
