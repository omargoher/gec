using FluentValidation;
namespace GEC.ApplicationCore.DTOs.Products;

public class UpdateVariantPriceRequestValidator : AbstractValidator<UpdateVariantPriceRequest>
{
    public UpdateVariantPriceRequestValidator()
    {
        RuleFor(x => x.PriceAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price must be zero or greater.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3)
            .WithMessage("Currency must be a 3-character ISO code.");
    }
}
