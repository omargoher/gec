# Configuration Guide

## Introduction
This guide provides an overview of how configuration options are structured and bound to strongly typed classes using the IOptions pattern in the GEC project.

## Configuration Style
- We use the `IOptions` pattern to bind settings to strongly typed classes.
- The configuration classes are located in the `src/backend/GEC.ApplicationCore/Options/` folder.

Example configuration class (`TestOptions.cs`):
```csharp
namespace GEC.ApplicationCore.Options;

public class TestOptions
{
    public string TestConfig { get; set; } = null!;
}
```

To register this configuration class, bind it in the infrastructure or presentation layer (like `GEC.Infrastructure/DependencyInjection.cs` or `Program.cs`) using the `services.Configure<T>` method.

Like this:
```csharp
services.Configure<TestOptions>(
    configuration.GetSection("TestSettings")); // "TestSettings" is the section name in appsettings.json
```

### Read Settings from appsettings.json
In the `appsettings.json` file, define a section matching the section name (`"TestSettings"`):
```json
{
  "TestSettings": {
    "TestConfig": "Some test config value"
  }
}
```

### Override Settings with Environment Variables
To override configuration settings using environment variables (for Docker/CI/CD pipelines), use the double underscore (`__`) naming convention matching the hierarchy.

For example, to override `TestConfig` inside `TestSettings`, define the environment variable:
```bash
TestSettings__TestConfig="New overridden value"
```
In `docker-compose.yml`, this is specified under environments:
```yaml
services:
  backend:
    environment:
      - TestSettings__TestConfig=${TEST_CONFIG_ENV}
```

## Extending Configuration
To add configuration:
1. Create a new Options class under `GEC.ApplicationCore/Options/`.
2. Add the matching JSON section in `appsettings.json` / `appsettings.Development.json`.
3. Register the binding using `services.Configure<TOptions>(configuration.GetSection("SectionName"))` inside `DependencyInjection.cs`.
4. Inject `IOptions<TOptions>` into your services.

## Use Configuration in Services or Controllers
Inject `IOptions<T>` into your service constructor and access configuration values through the `.Value` property.

For example:
```csharp
using GEC.ApplicationCore.Options;
using Microsoft.Extensions.Options;

public class TestUserService : ITestUserService
{
    private readonly TestOptions _options;

    public TestUserService(IOptions<TestOptions> options)
    {
        _options = options.Value;
    }

    public void DoSomething()
    {
        var config = _options.TestConfig;
        // Use config here...
    }
}
```
