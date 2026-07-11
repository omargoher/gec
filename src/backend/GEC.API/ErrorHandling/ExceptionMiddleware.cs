using FluentValidation;
using GEC.ApplicationCore.DTOs.Errors;
using GEC.ApplicationCore.Exceptions;

namespace GEC.API.ErrorHandling;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (Exception ex)
        {
            await HandleAsync(ctx, ex);
        }
    }

    private async Task HandleAsync(HttpContext ctx, Exception ex)
    {
        var response = ex switch
        {
            AppException e => new ErrorResponse
            {
                StatusCode = e.StatusCode,
                ErrorCode = e.ErrorCode,
                Message = e.Message,
                Errors = e.Errors
            },
            ValidationException ve => new ErrorResponse
            {
                StatusCode = 400,
                ErrorCode = ErrorCodes.ValidationError,
                Message = "Validation failed",
                Errors = ve.Errors
                    .GroupBy(f => f.PropertyName)
                    .Select(g => new FieldError
                    {
                        Field = g.Key,
                        Message = string.Join(" ", g.Select(f => f.ErrorMessage))
                    })
                    .ToList()
            },
            _ => new ErrorResponse
            {
                StatusCode = 500,
                ErrorCode = ErrorCodes.InternalServerError,
                Message = "An unexpected error occurred"
            }
        };

        response.TraceId = ctx.TraceIdentifier;

        if (response.StatusCode == 500)
            _logger.LogError(ex, "Unhandled exception");

        ctx.Response.StatusCode = response.StatusCode;
        ctx.Response.ContentType = "application/json";

        await ctx.Response.WriteAsJsonAsync(response);
    }
}