using FluentValidation;
namespace GEC.ApplicationCore.DTOs.Products;

public class AssignVariantAttributeValueRequestValidator : AbstractValidator<AssignVariantAttributeValueRequest>
{
    public AssignVariantAttributeValueRequestValidator()
    {
        RuleFor(x => x.AttributeId)
            .NotEqual(Guid.Empty)
            .WithMessage("AttributeId must not be empty.");

        RuleFor(x => x.AttributeValueId)
            .NotEqual(Guid.Empty)
            .WithMessage("AttributeValueId must not be empty.");
    }
}
