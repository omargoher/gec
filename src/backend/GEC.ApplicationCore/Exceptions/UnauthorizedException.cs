using GEC.ApplicationCore.DTOs.Errors;

namespace GEC.ApplicationCore.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException()
        : base(
            message: "You are not authenticated.",
            errorCode: ErrorCodes.Unauthorized,
            statusCode: 401)
    {
    }
}