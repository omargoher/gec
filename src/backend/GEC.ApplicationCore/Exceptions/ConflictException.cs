namespace GEC.ApplicationCore.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(string resource)
        : base(
            title: "Conflict",
            detail: $"{resource} already exists.",
            statusCode: 409)
    {
    }
}