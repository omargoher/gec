using FluentValidation;
using GEC.Domain.Enums;
namespace GEC.ApplicationCore.DTOs.Products;

public class ChangeVariantStatusRequestValidator : AbstractValidator<ChangeVariantStatusRequest>
{
    public ChangeVariantStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Status must be a valid ProductVariantStatus value.");
    }
}
