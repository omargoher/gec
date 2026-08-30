namespace GEC.ApplicationCore.Exceptions;

/// <summary>
/// Thrown when a write operation detects a stale concurrency token (RowVersion/xmin mismatch).
/// Maps to HTTP 409 Conflict.
/// </summary>
public class ConcurrencyException : AppException
{
    public ConcurrencyException()
        : base(
            title: "Concurrency Conflict",
            detail: "The resource was modified by another request. Please reload and retry.",
            statusCode: 409)
    {
    }

    public ConcurrencyException(string resource)
        : base(
            title: "Concurrency Conflict",
            detail: $"{resource} was modified by another request. Please reload and retry.",
            statusCode: 409)
    {
    }
}
