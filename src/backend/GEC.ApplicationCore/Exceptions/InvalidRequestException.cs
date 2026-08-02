namespace GEC.ApplicationCore.Exceptions;

public class InvalidRequestException : AppException
{
    public InvalidRequestException(string detail)
        : base(
            title: "Invalid Request",
            detail: detail,
            statusCode: 400)
    { }
}