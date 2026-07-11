# Exception & Error Handling Guide

This guide describes how the application handles errors and exceptions globally, and how to define new business exceptions.

---

## 1. Error Response Architecture

We return a consistent, structured error object to clients whenever something goes wrong (validation failures, database errors, domain rule violations).

### Standard JSON Error Response
```json
{
  "statusCode": 404,
  "errorCode": "NOT_FOUND",
  "message": "User was not found.",
  "traceId": "0HN12345ABCDE:00000001",
  "errors": []
}
```

If validation fails, the `errors` array is populated with specific field failures:
```json
{
  "statusCode": 400,
  "errorCode": "VALIDATION_ERROR",
  "message": "Validation failed",
  "traceId": "0HN12345ABCDE:00000002",
  "errors": [
    {
      "field": "Email",
      "message": "Email is not a valid email address."
    }
  ]
}
```

---

## 2. Base Exception: `AppException`

All custom domain and business exceptions must inherit from the abstract class `AppException` in [AppException.cs](file:///home/omar/RiderProjects/gec/src/backend/GEC.ApplicationCore/Exceptions/AppException.cs).

```csharp
using GEC.ApplicationCore.DTOs.Errors;

namespace GEC.ApplicationCore.Exceptions;

public abstract class AppException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }
    public List<FieldError> Errors { get; }
    
    protected AppException(string message, int statusCode = 500, string errorCode = ErrorsCode.InternalServerError, List<FieldError>? errors = null)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Errors = errors ?? [];
    }
}
```

---

## 3. How to Add a Custom Exception

To add a new custom exception:
1.  **Define a Code**: Add a constant key inside the static class `ErrorsCode` in [ErrorsCode.cs](file:///home/omar/RiderProjects/gec/src/backend/GEC.ApplicationCore/DTOs/Errors/ErrorsCode.cs) (e.g. `public const string OutOfStock = "OUT_OF_STOCK";`).
2.  **Create the Exception Class**: Create a class under `src/backend/GEC.ApplicationCore/Exceptions/` inheriting from `AppException`. Pass the message, status code, and error code to the base constructor.

### Example: Creating `OutOfStockException.cs`
```csharp
using GEC.ApplicationCore.DTOs.Errors;

namespace GEC.ApplicationCore.Exceptions;

public class OutOfStockException : AppException
{
    public OutOfStockException(string productName)
        : base(
            message: $"Product '{productName}' is currently out of stock.",
            statusCode: 400,
            errorCode: "OUT_OF_STOCK")
    {
    }
}
```

---

## 4. Throwing Custom Exceptions

Throw your exceptions directly inside the application core services. The presentation layer automatically captures them and handles serialization:

```csharp
public async Task OrderProductAsync(Guid productId, int quantity)
{
    var product = await _unitOfWork.Product.GetByIdAsync(productId);
    if (product == null)
    {
        throw new NotFoundException("Product");
    }

    if (product.StockQuantity < quantity)
    {
        throw new OutOfStockException(product.Name);
    }
    
    // Process order...
}
```

---

## 5. Global Exception Middleware

The API uses [ExceptionMiddleware.cs](file:///home/omar/RiderProjects/gec/src/backend/GEC.API/ErrorHandling/ExceptionMiddleware.cs) to catch all unhandled exceptions:

*   **`AppException`**: Maps to its defined `StatusCode` and `ErrorCode`.
*   **`ValidationException` (FluentValidation)**: Maps to `400 Bad Request` with `VALIDATION_ERROR` and groups failures by property name.
*   **Uncaught System Exceptions**: Maps to `500 Internal Server Error` with `INTERNAL_SERVER_ERROR`. These are automatically logged with full stack traces, keeping raw database or framework details hidden from clients.
