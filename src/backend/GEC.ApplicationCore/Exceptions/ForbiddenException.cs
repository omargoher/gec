namespace GEC.ApplicationCore.Exceptions;

public class ForbiddenException : AppException
{
    public ForbiddenException(string detail = "You do not have permission to access this resource.")
        : base(
            title: "Forbidden",
            detail: detail,
            statusCode: 403)
    {
    }
}