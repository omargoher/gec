using FluentValidation;
namespace GEC.ApplicationCore.DTOs.Attributes;

public class CreateAttributeValueRequestValidator : AbstractValidator<CreateAttributeValueRequest>
{
    public CreateAttributeValueRequestValidator()
    {
        RuleFor(x => x.Value)
            .NotEmpty()
            .MaximumLength(100);
    }
}
