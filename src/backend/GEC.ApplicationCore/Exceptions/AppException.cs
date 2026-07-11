using GEC.ApplicationCore.DTOs.Errors;

namespace GEC.ApplicationCore.Exceptions;

public abstract class AppException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }
    public List<FieldError> Errors { get; }

    protected AppException(string message, int statusCode = 500, string errorCode = ErrorCodes.InternalServerError, List<FieldError>? errors = null)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Errors = errors ?? [];
    }
}