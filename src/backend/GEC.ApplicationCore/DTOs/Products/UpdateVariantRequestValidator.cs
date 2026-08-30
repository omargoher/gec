using FluentValidation;
namespace GEC.ApplicationCore.DTOs.Products;

public class UpdateVariantRequestValidator : AbstractValidator<UpdateVariantRequest>
{
    private static readonly System.Text.RegularExpressions.Regex SkuRegex =
        new(@"^[A-Za-z0-9\-_]+$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public UpdateVariantRequestValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(64)
            .Matches(SkuRegex)
            .WithMessage("SKU must contain only letters, digits, hyphens, or underscores.")
            .When(x => x.Sku != null);

        RuleFor(x => x.PriceAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price must be zero or greater.")
            .When(x => x.PriceAmount.HasValue);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3)
            .WithMessage("Currency must be a 3-character ISO code.")
            .When(x => x.Currency != null);
    }
}
