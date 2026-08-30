using FluentValidation;
using GEC.Domain.Enums;
namespace GEC.ApplicationCore.DTOs.Products;

public class ChangeProductStatusRequestValidator : AbstractValidator<ChangeProductStatusRequest>
{
    public ChangeProductStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Status must be a valid ProductStatus value.");
    }
}
