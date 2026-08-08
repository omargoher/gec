using GEC.ApplicationCore.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GEC.API.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _env;
    private readonly IProblemDetailsService _problemDetailsService;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment env,
        IProblemDetailsService problemDetailsService)
    {
        _logger = logger;
        _env = env;
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            AppException e => (e.StatusCode, e.Title),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        if (exception is AppException)
        {
            _logger.LogWarning(
                exception,
                "Handled application exception {ExceptionType} [{StatusCode}]: {Message}",
                exception.GetType().Name, statusCode, exception.Message);
        }
        else
        {
            _logger.LogError(exception,
                "Unhandled exception [{StatusCode}]: {Message}",
                statusCode, exception.Message);
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Request.Headers.Accept = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Title = title,
            Status = statusCode,
            Detail = exception is AppException
                ? exception.Message
                : _env.IsDevelopment()
                    ? exception.Message
                    : "Please contact support if the problem persists.",
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
        };


        var written = await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });

        if (!written)
        {
            httpContext.Response.ContentType = "application/problem+json";
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        }

        return true;
    }
}