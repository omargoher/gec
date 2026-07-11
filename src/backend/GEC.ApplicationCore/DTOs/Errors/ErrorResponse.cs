namespace GEC.ApplicationCore.DTOs.Errors;

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string ErrorCode { get; set; } = ErrorCodes.InternalServerError;
    public string Message { get; set; } = string.Empty;
    public string? TraceId { get; set; }
    public List<FieldError> Errors { get; set; } = [];
}

