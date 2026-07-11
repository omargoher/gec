using GEC.ApplicationCore.DTOs.Errors;

namespace GEC.ApplicationCore.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string resource)
        : base(
            message: $"{resource} was not found.",
            errorCode: ErrorCodes.NotFound,
            statusCode: 404)
    {
    }
}