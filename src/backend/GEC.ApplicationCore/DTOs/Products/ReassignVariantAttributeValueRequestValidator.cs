using FluentValidation;
namespace GEC.ApplicationCore.DTOs.Products;

public class ReassignVariantAttributeValueRequestValidator : AbstractValidator<ReassignVariantAttributeValueRequest>
{
    public ReassignVariantAttributeValueRequestValidator()
    {
        RuleFor(x => x.AttributeId)
            .NotEqual(Guid.Empty)
            .WithMessage("AttributeId must not be empty.");

        RuleFor(x => x.NewAttributeValueId)
            .NotEqual(Guid.Empty)
            .WithMessage("NewAttributeValueId must not be empty.");
    }
}
