namespace GEC.ApplicationCore.Exceptions;

/// <summary>
/// Base class for all known/expected application exceptions.
/// Any exception NOT inheriting from this is treated as unexpected (500).
/// </summary>
public abstract class AppException : Exception
{
    public int StatusCode { get; }
    public string Title { get; }

    protected AppException(string title, string detail, int statusCode)
        : base(detail)
    {
        Title = title;
        StatusCode = statusCode;
    }
}