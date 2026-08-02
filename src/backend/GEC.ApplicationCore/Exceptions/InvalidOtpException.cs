
namespace GEC.ApplicationCore.Exceptions;

public class InvalidOtpException : AppException
{
    public InvalidOtpException(string detail)
        : base(
            title: "Invalid OTP",
            detail: detail,
            statusCode: 400)
    { }
}