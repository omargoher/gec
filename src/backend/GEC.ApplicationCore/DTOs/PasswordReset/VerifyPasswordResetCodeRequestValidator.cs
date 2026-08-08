using FluentValidation;

namespace GEC.ApplicationCore.DTOs.PasswordReset;

public class VerifyPasswordResetCodeRequestValidator : AbstractValidator<VerifyPasswordResetCodeRequest>
{
    public VerifyPasswordResetCodeRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Code).NotEmpty().Length(6).Matches("^[0-9]+$").WithMessage("Verification code must be a 6-digit number.");
    }
}