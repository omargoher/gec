namespace GEC.ApplicationCore.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string resource)
        : base(
            title: "Resource not found",
            detail: $"{resource} was not found.",
            statusCode: 404)
    {
    }

    public NotFoundException(string resource, object key)
        : base(
            title: "Resource not found",
            detail: $"{resource} (\"{key}\") was not found.",
            statusCode: 404)
    {
    }
}