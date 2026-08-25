using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using GEC.API.Services;
using GEC.ApplicationCore.Interfaces.Identity;
using GEC.ApplicationCore.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

namespace GEC.API;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.AddHttpContextAccessor();
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssembly(typeof(ApplicationCore.DependencyInjection).Assembly);

        AddSwagger(services);

        services.AddAuthorization();

        AddCors(services, configuration);

        AddJwtOptions(services, configuration);
        AddJwtAuthentication(services, configuration);

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICartResolver, CartResolver>();
        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt configuration section is missing.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue("accessToken", out var token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    }
                };

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        return services;
    }

    public static IServiceCollection AddCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigin = configuration["Cors:AllowedOrigin"]
                            ?? throw new InvalidOperationException("CORS AllowedOrigin is not configured.");

        services.AddCors(options =>
        {
            options.AddPolicy("ReactApp", policy =>
                policy.WithOrigins(allowedOrigin)
                    .AllowAnyHeader().AllowAnyMethod().AllowCredentials());
        });
        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        const string schemeId = "Bearer";

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "GEC API",
                Version = "v1",
                Description = "GEC is a Ganeral eCommerce System",
                Contact = new OpenApiContact { Name = "GEC Team" }
            });

            options.UseInlineDefinitionsForEnums();

            options.AddSecurityDefinition(schemeId, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter JWT token like: Bearer {your_token}"
            });

            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "GEC.API.xml"));
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "GEC.ApplicationCore.xml"));
        });

        return services;
    }

    public static IServiceCollection AddJwtOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        return services;
    }
}