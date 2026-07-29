# Controller Guide

This guide describes how to create and configure controllers in the **`GEC.API`** layer.

---

## 1. Naming & Route Conventions
*   **Location**: Controllers must be placed in `src/backend/GEC.API/Controllers/`.
*   **File Name**: Named `<FeatureName>Controller.cs` (e.g., `UsersController.cs`, `ProductsController.cs`).
*   **Routing**: Use standard RESTful lower-case plural naming conventions:
    *   Good: `[Route("api/users")]` or `[Route("api/[controller]")]`
    *   Avoid using action names in routes (e.g. avoid `api/users/get-all`). Use HTTP verbs (`GET`, `POST`, `PUT`, `DELETE`) to express the operation.

---

## 2. Controller Annotations & Attributes

Every controller must have the following base attributes:

*   `[ApiController]`: Enables API-specific behaviors (automatic 400 validation responses, parameter source binding inference, etc.).
*   `[Route("api/[controller]")]`: Configures base routes.
*   `[Tags("Tag Name")]`: Categorizes endpoints in Swagger UI.

Example base setup:
```csharp
using Microsoft.AspNetCore.Mvc;

namespace GEC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Products")]
public class ProductsController : ControllerBase
{
}
```

---

## 3. Swagger XML Comments & Descriptions

To display detailed descriptions, input explanations, and HTTP status codes in Swagger, write standard C# XML comments above actions.

### Key tags to use:
*   `<summary>`: A short summary of what the endpoint does.
*   `<remarks>`: Detailed descriptions (markdown supported), sample request bodies, or flow specifics.
*   `<param>`: Describes route, query, or body parameters.
*   `<response>`: Explains what each status code means.

Additionally, use `[ProducesResponseType]` to register response schema models for Swagger:

```csharp
    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/products
    ///     {
    ///        "name": "Mechanical Keyboard",
    ///        "price": 99.99
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">Payload needed to create the product.</param>
    /// <returns>The newly created product response.</returns>
    /// <response code="201">Returns the created product details.</response>
    /// <response code="400">Invalid payload or validation failed.</response>
    /// <response code="409">A product with the same SKU/name already exists.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateProductRequest request)
    {
        // code
    }
```

---

## 4. Standard Response Formats

Do not return raw domain entities or ad-hoc anonymous objects. Always return:
1.  **Successful Responses**: Standard DTOs (`ProductResponse`, `PagedResult<ProductResponse>`).
2.  **Error Responses**: Standard RFC 7807 `ProblemDetails` or `ValidationProblemDetails` (handled automatically by built-in validation or the global exception handler).
