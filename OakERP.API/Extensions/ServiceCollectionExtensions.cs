using System.Diagnostics;
using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using OakERP.API.Runtime;
using OakERP.API.Swagger.Filters.Auth;

namespace OakERP.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRuntimeSupport(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var authRateLimitSettings = AuthRateLimitSettings.Bind(configuration);
        var timeoutSettings = RequestTimeoutSettings.Bind(configuration);

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
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = WriteRateLimitProblemDetailsAsync;
            options.AddPolicy<string>(
                AuthRateLimitSettings.PolicyName,
                httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: GetAuthPartitionKey(httpContext),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = authRateLimitSettings.PermitLimit,
                            Window = TimeSpan.FromSeconds(authRateLimitSettings.WindowSeconds),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = authRateLimitSettings.QueueLimit,
                            AutoReplenishment = true,
                        }
                    )
            );
        });
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
            .AddCheck<DatabaseConnectivityHealthCheck>("database", tags: ["ready"]);
        services.AddRequestTimeouts(options =>
        {
            options.DefaultPolicy = new RequestTimeoutPolicy
            {
                Timeout = TimeSpan.FromSeconds(timeoutSettings.ControllerSeconds),
                TimeoutStatusCode = StatusCodes.Status503ServiceUnavailable,
                WriteTimeoutResponse = WriteTimeoutProblemDetailsAsync,
            };
        });

        return services;
    }

    private static string GetAuthPartitionKey(HttpContext context)
    {
        var remoteIpAddress =
            context.Connection.RemoteIpAddress?.ToString() ?? AuthRateLimitSettings.UnknownClientPartition;

        return $"{context.Request.Path}:{remoteIpAddress}";
    }

    private static async ValueTask WriteRateLimitProblemDetailsAsync(
        OnRejectedContext context,
        CancellationToken cancellationToken
    )
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers["Retry-After"] = Math.Ceiling(retryAfter.TotalSeconds)
                .ToString(CultureInfo.InvariantCulture);
        }

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        var problemDetailsService = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();

        await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = context.HttpContext,
                ProblemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Too many requests.",
                    Type = "https://httpstatuses.com/429",
                },
            }
        );
    }

    private static async Task WriteTimeoutProblemDetailsAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;

        var problemDetailsService = context.RequestServices.GetRequiredService<IProblemDetailsService>();

        await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status503ServiceUnavailable,
                    Title = "The request timed out.",
                    Type = "https://httpstatuses.com/503",
                },
            }
        );
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
