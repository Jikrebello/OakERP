using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OakERP.Common.Exceptions;

namespace OakERP.API.Extensions;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var correlationId = httpContext.GetCorrelationId();
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        using var scope = logger.BeginScope(
            new Dictionary<string, object?>
            {
                ["CorrelationId"] = correlationId,
                ["TraceId"] = traceId,
            }
        );

        ProblemDetails problemDetails = CreateProblemDetails(exception);

        LogException(exception, httpContext, problemDetails.Status ?? StatusCodes.Status500InternalServerError);

        return await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails,
                Exception = exception,
            }
        );
    }

    private ProblemDetails CreateProblemDetails(Exception exception)
    {
        if (exception is OakErpException oakErpException)
        {
            var statusCode = (int)oakErpException.StatusCode;
            return new ProblemDetails
            {
                Status = statusCode,
                Title = oakErpException.Title,
                Type = $"https://httpstatuses.com/{statusCode}",
                Detail = environment.IsDevelopment() ? oakErpException.Message : null,
            };
        }

        return new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Type = "https://httpstatuses.com/500",
            Detail = environment.IsDevelopment() ? exception.Message : null,
        };
    }

    private void LogException(Exception exception, HttpContext httpContext, int statusCode)
    {
        var path = httpContext.Request.Path.Value ?? "/";
        var method = httpContext.Request.Method;

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception for HTTP {Method} {Path}", method, path);
            return;
        }

        logger.LogWarning(exception, "Handled exception for HTTP {Method} {Path}", method, path);
    }
}
