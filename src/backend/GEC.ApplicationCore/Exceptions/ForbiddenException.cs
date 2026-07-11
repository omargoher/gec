using GEC.ApplicationCore.DTOs.Errors;

namespace GEC.ApplicationCore.Exceptions;

public class ForbiddenException : AppException
{
    public ForbiddenException()
        : base(
            message: "You do not have permission to access this resource.",
            errorCode: ErrorCodes.Forbidden,
            statusCode: 403)
    {
    }
}