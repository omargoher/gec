# Exception & Error Handling Guide

This guide describes how the application handles errors and exceptions globally using ASP.NET Core's standard exception handling features and how to define new business exceptions.

---

## 1. Error Response Architecture

We return a consistent, structured error object conforming to **RFC 7807 (Problem Details for HTTP APIs)** whenever something goes wrong (validation failures, database errors, domain rule violations).

### Standard JSON Error Response (Problem Details)
For expected domain exceptions (inheriting from `AppException`), the API returns the following format:
```json
{
  "title": "Resource not found",
  "status": 404,
  "detail": "Product was not found.",
  "instance": "POST /api/products",
  "traceId": "0HN12345ABCDE:00000001",
  "timestamp": "2026-07-30T01:12:32Z"
}
```

### Validation Error Response (Validation Problem Details)
If fluent validation fails, the framework automatically returns a `ValidationProblemDetails` response containing specific field errors:
```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "Validation failed",
  "instance": "POST /api/users",
  "traceId": "0HN12345ABCDE:00000002",
  "timestamp": "2026-07-30T01:13:00Z",
  "errors": {
    "Email": [
      "Email is not a valid email address."
    ]
  }
}
```
*Note: In `Development` environment, the `exceptionType` extension property is also attached to the response payload to assist in debugging.*

---

## 2. Base Exception: `AppException`

All custom domain and business exceptions must inherit from the abstract class `AppException` in [AppException.cs](../../backend/GEC.ApplicationCore/Exceptions/AppException.cs).

```csharp
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
```

---

## 3. How to Add a Custom Exception

To add a new custom exception:
1. **Create the Exception Class**: Create a class under `src/backend/GEC.ApplicationCore/Exceptions/` inheriting from `AppException`.
2. **Pass Parameters**: Pass a descriptive `title`, detail message (`detail`), and the appropriate HTTP `statusCode` to the base constructor.

### Example: Creating `OutOfStockException.cs`
```csharp
namespace GEC.ApplicationCore.Exceptions;

public class OutOfStockException : AppException
{
    public OutOfStockException(string productName)
        : base(
            title: "Out of Stock",
            detail: $"Product '{productName}' is currently out of stock.",
            statusCode: 400)
    {
    }
}
```

---

## 4. Throwing Custom Exceptions

Throw your exceptions directly inside the application core services. The presentation layer's global exception handler automatically catches them and handles mapping to HTTP responses:

```csharp
public async Task OrderProductAsync(Guid productId, int quantity)
{
    var product = await _unitOfWork.Product.GetByIdAsync(productId);
    if (product == null)
    {
        throw new NotFoundException("Product", productId);
    }

    if (product.StockQuantity < quantity)
    {
        throw new OutOfStockException(product.Name);
    }
    
    // Process order...
}
```

---

## 5. Global Exception Handler Pipeline

The API configures the exception handling pipeline in [Program.cs](../../backend/GEC.API/Program.cs):

```csharp
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
{
    context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    context.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow;

    if (builder.Environment.IsDevelopment())
    {
        context.ProblemDetails.Extensions["exceptionType"] = context.Exception?.GetType().Name;
    }
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
...
app.UseExceptionHandler();
app.UseStatusCodePages();
```

* **`AppException`**: Caught by [GlobalExceptionHandler.cs](../../backend/GEC.API/Handlers/GlobalExceptionHandler.cs). It maps the custom exception's `StatusCode` and `Title` to the HTTP response, logging it as a warning.
* **Uncaught System Exceptions**: Caught by `GlobalExceptionHandler` and mapped to a generic `500 Internal Server Error` with `An unexpected error occurred`. The original message is only shared in the `Detail` field if running in `Development` mode, and it is logged as an error with the full stack trace to protect system internals.
