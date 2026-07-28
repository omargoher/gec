using GEC.API;
using GEC.API.ErrorHandling;
using GEC.ApplicationCore;
using GEC.Infrastructure;
using Serilog;
using GEC.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

builder.Services
    .AddPresentation(builder.Configuration)
    .AddApplicationCore()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

// seed the roles
await app.Services.SeedRolesAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("ReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

