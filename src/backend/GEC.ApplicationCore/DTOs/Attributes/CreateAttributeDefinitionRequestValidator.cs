using FluentValidation;
namespace GEC.ApplicationCore.DTOs.Attributes;

public class CreateAttributeDefinitionRequestValidator : AbstractValidator<CreateAttributeDefinitionRequest>
{
    public CreateAttributeDefinitionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
