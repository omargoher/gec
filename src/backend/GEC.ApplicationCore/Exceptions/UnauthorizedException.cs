namespace GEC.ApplicationCore.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string detail = "You are not authenticated.")
        : base(
            title: "Unauthorized",
            detail: detail,
            statusCode: 401)
    {
    }
}