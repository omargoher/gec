namespace GEC.ApplicationCore.Exceptions;

public class DomainException : AppException
{
    public DomainException(string detail)
        : base(
            title: "Domain Rule Violation",
            detail: detail,
            statusCode: 400)
    { }
}