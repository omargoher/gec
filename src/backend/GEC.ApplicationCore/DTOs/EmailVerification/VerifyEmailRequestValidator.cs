using FluentValidation;
using GEC.ApplicationCore.DTOs.EmailVerification;

namespace GEC.ApplicationCore.DTOs.EmailVerification;

public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Code).NotEmpty().Length(6).Matches("^[0-9]+$").WithMessage("Verification code must be a 6-digit number.");
    }
}