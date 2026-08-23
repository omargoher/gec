# DTO Guide

## Naming
- DTOs should be named `<Name>Request.cs` or `<Name>Response.cs` depending on the use case, where `<Name>` is the name of the entity or operation they represent.
  - For example, if you have an operation called `CreateUser`, you can have `CreateUserRequest.cs` for input data, and `CreateUserResponse.cs` for output data.
- DTOs may also be named `<Name>Dto.cs` if they represent data transferred internally between layers rather than raw requests/responses.

## Type
- DTOs should be defined as `record` types to ensure immutability and value-based equality.
- Use Positional Record Types or Property-based Record Types depending on complexity:
  - **Positional Record Type** (concise, constructor-based):
    ```csharp
    public record CreateUserRequest(string FullName, string Email, string Password);
    ```
  - **Property-based Record Type** (easier for deserialization of complex structures):
    ```csharp
    public record UploadProductImagesRequest
    {
        public Guid Id { get; init; }
        public int Order { get; init; }
        public IReadOnlyList<IFormFile> Images { get; init; } = null!;
    }
    ```

## Validation
- For request DTOs, use **FluentValidation** to ensure data correctness before handling it in the application layer.
- Create a validator class for each request DTO in the same folder, named `<DtoName>Validator.cs`.
- For example, if you have `CreateCategoryRequest.cs`, create `CreateCategoryRequestValidator.cs` as follows:
```csharp
using FluentValidation;

namespace GEC.ApplicationCore.DTOs.Categories;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Name cannot be whitespace");

        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(100)
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug must be lowercase, hyphen-separated, and valid");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => x.Description != null);
    }
}
```

## Mapping
- For mapping between DTOs and domain entities, use **AutoMapper** to simplify conversions and reduce boilerplate code.
- Create a mapping profile class under `src/backend/GEC.ApplicationCore/Mappers/` and name it `<EntityName>Profile.cs`.
- For example, for the `Category` entity, create `CategoryProfile.cs`:
```csharp
using AutoMapper;
using GEC.ApplicationCore.DTOs.Categories;
using GEC.Domain.Entities;

namespace GEC.ApplicationCore.Mappers;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        // Entity → DTO 
        CreateMap<Category, CategoryResponse>();

        // Create DTO → Entity
        CreateMap<CreateCategoryRequest, Category>();
    }
}
```
All mapping profiles are automatically scanned and registered on startup.