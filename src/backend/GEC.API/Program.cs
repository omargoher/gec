using System.Security.Claims;
using GEC.API;
using GEC.API.Handlers;
using GEC.ApplicationCore;
using GEC.Infrastructure;
using Serilog;
using GEC.Infrastructure.Identity;
using GEC.Infrastructure.Persistence;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Templates;

var builder = WebApplication.CreateBuilder(args);

Serilog.Debugging.SelfLog.Enable(Console.Error);
builder.Services.AddSerilog((services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithSpan();
});

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



builder.Services
    .AddPresentation(builder.Configuration)
    .AddApplicationCore()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Ensure database schema is up-to-date
await app.Services.ApplyMigrationsAsync();

// Seed default data
await app.Services.SeedRolesAsync();
await app.Services.SeedAdminAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

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

app.UseHttpsRedirection();
app.UseCors("ReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

