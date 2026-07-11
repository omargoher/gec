using GEC.ApplicationCore.DTOs.Errors;

namespace GEC.ApplicationCore.Exceptions;

public class InvalidRequestException : AppException
{
    public InvalidRequestException(string message)
        : base(
            message: message,
            errorCode: ErrorCodes.InvalidRequest,
            statusCode: 400)
    { }
}