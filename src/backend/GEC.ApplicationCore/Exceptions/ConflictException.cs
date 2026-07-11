using GEC.ApplicationCore.DTOs.Errors;

namespace GEC.ApplicationCore.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(string resource)
        : base(message: $"{resource} already exists.",
            errorCode: ErrorCodes.Conflict,
            statusCode: 409)
    {
    }
}