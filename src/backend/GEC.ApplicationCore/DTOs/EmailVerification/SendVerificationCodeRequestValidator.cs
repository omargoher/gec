using FluentValidation;
using GEC.ApplicationCore.DTOs.EmailVerification;

namespace GEC.ApplicationCore.DTOs.EmailVerification;

public class SendVerificationCodeRequestValidator : AbstractValidator<SendVerificationCodeRequest>
{
    public SendVerificationCodeRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}