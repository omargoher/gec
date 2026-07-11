using FluentValidation;
using FluentValidation.Results;
using GEC.ApplicationCore.DTOs.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Results;

namespace GEC.API.ErrorHandling;

public class CustomResultFactory : IFluentValidationAutoValidationResultFactory
{
    public Task<IActionResult?> CreateActionResult(
        ActionExecutingContext context,
        ValidationProblemDetails? validationProblemDetails,
        IDictionary<IValidationContext, ValidationResult> validationResults)
    {
        var errors = validationResults
            .SelectMany(kvp => kvp.Value.Errors)
            .GroupBy(e => e.PropertyName)
            .Select(g => new FieldError
            {
                Field = g.Key,
                Message = string.Join(" ", g.Select(e => e.ErrorMessage))
            })
            .ToList();

        var response = new ErrorResponse
        {
            StatusCode = 400,
            ErrorCode = ErrorCodes.ValidationError,
            Message = "Validation failed",
            TraceId = context.HttpContext.TraceIdentifier,
            Errors = errors
        };

        IActionResult result = new BadRequestObjectResult(response);

        return Task.FromResult<IActionResult?>(result);
    }
}