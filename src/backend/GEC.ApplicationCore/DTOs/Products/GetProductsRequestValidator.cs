using FluentValidation;
namespace GEC.ApplicationCore.DTOs.Products;

public class GetProductsRequestValidator : AbstractValidator<GetProductsRequest>
{
    public GetProductsRequestValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(100)
            .When(x => x.Search != null);
    }
}
