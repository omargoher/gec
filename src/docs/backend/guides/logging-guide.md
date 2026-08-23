# Logging & Serilog Guide

This guide describes how logging is architected and configured within the **GEC** application, including sink configurations, enrichers, request logging, and structured logging guidelines.

---

## 1. Serilog Bootstrapping & Registration

We use **Serilog** for structured logging instead of the default ASP.NET Core logging providers. It is configured in [Program.cs](../../backend/GEC.API/Program.cs) as follows:

```csharp
Serilog.Debugging.SelfLog.Enable(Console.Error);
builder.Services.AddSerilog((services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithSpan();
});
```

*   **`ReadFrom.Configuration`**: Reads logging configuration directly from `appsettings.json` and environment variables.
*   **`Enrich.FromLogContext`**: Dynamically enriches log events with properties from the `LogContext` (e.g., Correlation IDs, HTTP Request details).
*   **`Enrich.WithSpan`**: Integrates with `.NET Activity` / OpenTelemetry to inject tracing spans (`TraceId`, `SpanId`, `ParentId`) into the logs.

---

## 2. Configuration & Sinks

Logging settings are defined inside `appsettings.json` (Production defaults) and `appsettings.Development.json` (Local developer setup).

### A. Development Settings (`appsettings.Development.json`)
In development, logs are output in a human-readable expression template format to the console, and sent as structured payloads to **Seq**:

```json
{
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.Seq", "Serilog.Expressions" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore.Database.Command": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "formatter": {
            "type": "Serilog.Templates.ExpressionTemplate, Serilog.Expressions",
            "template": "[{@t:HH:mm:ss} {@l:u3}] {#if TraceId is not null}[TraceId:{TraceId}] {#end}{@m}\n{@x}"
          }
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://seq:80"
        }
      }
    ]
  }
}
```

*   **Console Expression Template**: Prettifies console logs and prefixes them with the active `TraceId` if it is present in the context.
*   **Seq Sink**: Forwards logs to the Seq log aggregation server at `http://seq:80`.

### B. Production Settings (`appsettings.json`)
In production, console logs are written in a structured JSON format suitable for container log collectors (like Fluentd or Promtail):

```json
{
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.Seq", "Serilog.Formatting.Compact" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "System": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "formatter": {
            "type": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
          }
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://seq:80"
        }
      }
    ]
  }
}
```

---

## 3. Seq Log Server & Docker Compose

We run **Seq** locally inside a docker container. It is configured in [docker-compose.yml](../../../docker-compose.yml):

```yaml
  seq:
    image: datalust/seq:latest
    container_name: gec-seq
    restart: always
    environment:
      ACCEPT_EULA: "Y"
      SEQ_FIRSTRUN_ADMINPASSWORD: "Admin@gec"
    ports:
      - "5341:80"
    volumes:
      - seq_data:/data
```

*   **Web UI**: Accessible at `http://localhost:5341` (mapped from port 80).
*   **Internal Service URL**: `http://seq:80` for backend container to container communications.

---

## 4. HTTP Request & User ID Logging

We intercept and log all incoming HTTP requests via `SerilogRequestLogging`. It is customized in `Program.cs` to enrich log details with the authenticated User ID:

```csharp
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        var userId = httpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            diagnosticContext.Set("UserId", userId);
        }
    };
});
```

This adds a `UserId` property to request summary logs (like `HTTP GET /api/products responded 200 in 15.2 ms`), allowing you to filter user activities easily in Seq.

---

## 5. Structured Logging Best Practices

### A. Prefer Message Templates Over String Interpolation
Do **NOT** use C# string interpolation (`$""`) when logging variables. Doing so burns the values directly into the message text, making indexing and searching impossible. Always use structured message templates:

```csharp
// BAD - Not searchable, burns string
_logger.LogInformation($"Successfully updated stock for product {productId} to {quantity}.");

// GOOD - Creates searchable properties 'ProductId' and 'Quantity'
_logger.LogInformation("Successfully updated stock for product {ProductId} to {Quantity}.", productId, quantity);
```

### B. Standard Log Level Usage
*   **`Debug` / `Verbose`**: Internal system diagnostics, SQL command parameters, or noisy execution steps.
*   **`Information`**: Core application milestones. e.g., "User logged in", "Order placed", "Database migrations completed".
*   **`Warning`**: Expected non-critical anomalies. e.g., Validation failures, handled domain exceptions (like `NotFoundException`), resource throttling.
*   **`Error`**: Unhandled system exceptions, integration failures, or database connectivity losses that require administrative attention.
*   **`Critical` / `Fatal`**: Critical crashes causing application shutdown.
