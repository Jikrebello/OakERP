using System.Diagnostics;
using Microsoft.OpenApi.Models;
using OakERP.API.Swagger.Filters.Auth;

namespace OakERP.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRuntimeSupport(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                var httpContext = context.HttpContext;

                context.ProblemDetails.Instance ??= httpContext.Request.Path;
                context.ProblemDetails.Extensions["traceId"] =
                    Activity.Current?.Id ?? httpContext.TraceIdentifier;
                context.ProblemDetails.Extensions["correlationId"] = httpContext.GetCorrelationId();
            };
        });

        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }

    public static IServiceCollection AddSwaggerDocs(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "OakERP API", Version = "v1" });

            // DTO Example Filters
            options.SchemaFilter<RegisterDtoExampleFilter>();

            options.SchemaFilter<LoginDtoExampleFilter>();

            // Jwt Support
            options.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your token like this: Bearer {your JWT token}",
                }
            );

            options.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                        },
                        Array.Empty<string>()
                    },
                }
            );
        });

        return services;
    }
}
