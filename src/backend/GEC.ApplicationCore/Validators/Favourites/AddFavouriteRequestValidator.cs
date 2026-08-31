using FluentValidation;
using GEC.ApplicationCore.DTOs.Favourites;

namespace GEC.ApplicationCore.Validators.Favourites;

/// <summary>
/// Validates the shape of an AddFavouriteRequest.
/// Does NOT check product existence, product status, or duplicate favourites —
/// those are business concerns handled by FavouriteService.
/// </summary>
public sealed class AddFavouriteRequestValidator : AbstractValidator<AddFavouriteRequest>
{
    public AddFavouriteRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId is required.");
    }
}
