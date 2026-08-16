using FluentValidation;

namespace GEC.ApplicationCore.DTOs.PasswordReset;

public class SendPasswordResetCodeRequestValidator : AbstractValidator<SendPasswordResetCodeRequest>
{
    public SendPasswordResetCodeRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}