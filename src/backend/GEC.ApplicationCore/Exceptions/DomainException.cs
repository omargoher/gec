using GEC.ApplicationCore.DTOs.Errors;

namespace GEC.ApplicationCore.Exceptions;

public class DomainException : AppException
{
    public DomainException(string message)
        : base(
            message: message,
            errorCode: ErrorCodes.DomainRuleViolation,
            statusCode: 400)
    { }
}