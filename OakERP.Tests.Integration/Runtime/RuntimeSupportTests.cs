using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using OakERP.API.Extensions;
using OakERP.API.Runtime;
using OakERP.Common.Dtos.Auth;
using OakERP.Infrastructure.Persistence;
using Shouldly;

namespace OakERP.Tests.Integration.Runtime;

[TestFixture]
public class RuntimeSupportTests : WebApiIntegrationTestBase
{
    private const string CorrelationHeaderName = "X-Correlation-ID";

    [Test]
    public async Task Unauthenticated_Request_Should_Return_ProblemDetails_With_Generated_Correlation_Id()
    {
        var response = await Client.GetAsync("/api/users/whoami");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var correlationId = response.Headers.GetValues(CorrelationHeaderName).Single();
        correlationId.ShouldNotBeNullOrWhiteSpace();

        var body = await RuntimeSupportTestJson.ReadJsonAsync(response);
        body.GetProperty("status").GetInt32().ShouldBe((int)HttpStatusCode.Unauthorized);
        body.GetProperty("correlationId").GetString().ShouldBe(correlationId);
        body.GetProperty("traceId").GetString().ShouldNotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task Business_Failure_Should_Preserve_Dto_Body_And_Echo_Inbound_Correlation_Id()
    {
        const string correlationId = "runtime-test-correlation";

        using var request = new HttpRequestMessage(HttpMethod.Post, ApiRoutes.Auth.Login)
        {
            Content = JsonContent.Create(
                new LoginDto { Email = "missing@example.com", Password = "bad-password" }
            ),
        };

        request.Headers.Add(CorrelationHeaderName, correlationId);

        var response = await Client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
        response.Headers.GetValues(CorrelationHeaderName).Single().ShouldBe(correlationId);

        var result = await response.Content.ReadFromJsonAsync<AuthResultDto>();
        result.ShouldNotBeNull();
        result.Success.ShouldBeFalse();
        result.Message.ShouldBe("Invalid login credentials.");
    }

    [Test]
    public async Task Auth_Audit_Log_Should_Preserve_Correlation_Context_For_Login_Failure()
    {
        const string correlationId = "audit-runtime-correlation";
        using var loggerProvider = new InMemoryLoggerProvider();

        using var overrideFactory = Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddProvider(loggerProvider);
                logging.SetMinimumLevel(LogLevel.Information);
            });
        });

        using var client = overrideFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, ApiRoutes.Auth.Login)
        {
            Content = JsonContent.Create(
                new LoginDto { Email = "missing@example.com", Password = "bad-password" }
            ),
        };

        request.Headers.Add(CorrelationHeaderName, correlationId);

        var response = await client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");

        var auditLog = loggerProvider
            .Entries.Where(entry => entry.Category == typeof(AuthService).FullName)
            .Single(entry =>
                entry.Level == LogLevel.Warning
                && entry.Properties.TryGetValue("AuditAction", out var auditAction)
                && Equals(auditAction, "UserLogin")
                && entry.Properties.TryGetValue("AuditReason", out var auditReason)
                && Equals(auditReason, "InvalidCredentials")
            );

        RuntimeSupportTestJson.GetContextValue(auditLog, "CorrelationId").ShouldBe(correlationId);
        RuntimeSupportTestJson.GetContextValue(auditLog, "TraceId").ShouldNotBeNullOrWhiteSpace();
        auditLog.Properties["Email"].ShouldBe("missing@example.com");
    }

    [Test]
    public async Task Runtime_Logs_Should_Be_Forwarded_To_Added_Providers()
    {
        const string correlationId = "provider-forwarding-correlation";
        using var loggerProvider = new InMemoryLoggerProvider();

        using var overrideFactory = Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddProvider(loggerProvider);
                logging.SetMinimumLevel(LogLevel.Information);
            });
        });

        using var client = overrideFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, ApiRoutes.Auth.Login)
        {
            Content = JsonContent.Create(
                new LoginDto { Email = "missing@example.com", Password = "bad-password" }
            ),
        };

        request.Headers.Add(CorrelationHeaderName, correlationId);

        var response = await client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var requestLog = loggerProvider
            .Entries.Where(entry =>
                entry.Category == "OakERP.API.Extensions.RequestLoggingMiddleware"
            )
            .Single(entry =>
                entry.Level == LogLevel.Information
                && entry.Properties.TryGetValue("Method", out var method)
                && Equals(method, "POST")
                && entry.Properties.TryGetValue("Path", out var path)
                && Equals(path, "/api/auth/login")
                && entry.Properties.TryGetValue("StatusCode", out var statusCode)
                && Equals(statusCode, 401)
            );

        RuntimeSupportTestJson.GetContextValue(requestLog, "CorrelationId").ShouldBe(correlationId);
        RuntimeSupportTestJson.GetContextValue(requestLog, "TraceId").ShouldNotBeNullOrWhiteSpace();

        var auditLog = loggerProvider
            .Entries.Where(entry => entry.Category == typeof(AuthService).FullName)
            .Single(entry =>
                entry.Level == LogLevel.Warning
                && entry.Properties.TryGetValue("AuditAction", out var auditAction)
                && Equals(auditAction, "UserLogin")
            );

        RuntimeSupportTestJson.GetContextValue(auditLog, "CorrelationId").ShouldBe(correlationId);
        RuntimeSupportTestJson.GetContextValue(auditLog, "TraceId").ShouldNotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task Unhandled_Exception_Should_Return_ProblemDetails_With_Correlation_Id()
    {
        const string correlationId = "exception-correlation-id";

        using var overrideFactory = Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped<IAuthService, ThrowingAuthService>();
            });
        });

        using var client = overrideFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, ApiRoutes.Auth.Login)
        {
            Content = JsonContent.Create(
                new LoginDto { Email = "boom@example.com", Password = "ignored" }
            ),
        };

        request.Headers.Add(CorrelationHeaderName, correlationId);

        var response = await client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        response.Headers.GetValues(CorrelationHeaderName).Single().ShouldBe(correlationId);

        var body = await RuntimeSupportTestJson.ReadJsonAsync(response);
        body.GetProperty("status").GetInt32().ShouldBe((int)HttpStatusCode.InternalServerError);
        body.GetProperty("title").GetString().ShouldBe("An unexpected error occurred.");
        body.GetProperty("correlationId").GetString().ShouldBe(correlationId);
        body.GetProperty("traceId").GetString().ShouldNotBeNullOrWhiteSpace();
        body.ToString().ShouldNotContain("Simulated login failure");
    }

    [Test]
    public async Task Configuration_Exception_Should_Return_ProblemDetails_With_Config_Title()
    {
        using var overrideFactory = Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped<IAuthService, ConfigurationFailingAuthService>();
            });
        });

        using var client = overrideFactory.CreateClient();
        var response = await client.PostAsJsonAsync(
            ApiRoutes.Auth.Login,
            new LoginDto { Email = "config@example.com", Password = "ignored" }
        );

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await RuntimeSupportTestJson.ReadJsonAsync(response);
        body.GetProperty("status").GetInt32().ShouldBe(StatusCodes.Status500InternalServerError);
        body
            .GetProperty("title")
            .GetString()
            .ShouldBe("Application configuration is invalid.");
    }

    private sealed class ThrowingAuthService : IAuthService
    {
        public Task<AuthResultDto> RegisterAsync(RegisterDto Dto)
        {
            throw new InvalidOperationException("Simulated registration failure");
        }

        public Task<AuthResultDto> LoginAsync(LoginDto Dto)
        {
            throw new InvalidOperationException("Simulated login failure");
        }
    }

    private sealed class ConfigurationFailingAuthService : IAuthService
    {
        public Task<AuthResultDto> RegisterAsync(RegisterDto Dto)
        {
            throw new ConfigurationValidationException(
                "Auth:Test",
                "Simulated registration configuration failure"
            );
        }

        public Task<AuthResultDto> LoginAsync(LoginDto Dto)
        {
            throw new ConfigurationValidationException(
                "Auth:Test",
                "Simulated login configuration failure"
            );
        }
    }
}


internal static class RuntimeSupportTestJson
{
    public static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.Clone();
    }

    public static string? GetContextValue(CapturedLogEntry entry, string propertyName)
    {
        if (entry.ScopeProperties.TryGetValue(propertyName, out var scopeValue))
        {
            return scopeValue?.ToString();
        }

        return entry.Properties.TryGetValue(propertyName, out var propertyValue)
            ? propertyValue?.ToString()
            : null;
    }
}

internal sealed record CapturedLogEntry(
    string Category,
    LogLevel Level,
    string Message,
    IReadOnlyDictionary<string, object?> Properties,
    IReadOnlyDictionary<string, object?> ScopeProperties
);

internal sealed class InMemoryLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly ConcurrentQueue<CapturedLogEntry> entries = new();
    private IExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();

    public IReadOnlyCollection<CapturedLogEntry> Entries => [.. entries];

    public ILogger CreateLogger(string categoryName)
    {
        return new InMemoryLogger(categoryName, entries, () => scopeProvider);
    }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        this.scopeProvider = scopeProvider;
    }

    public void Dispose() { }
}

internal sealed class InMemoryLogger(
    string categoryName,
    ConcurrentQueue<CapturedLogEntry> entries,
    Func<IExternalScopeProvider> getScopeProvider
) : ILogger
{
    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull
    {
        return getScopeProvider().Push(state);
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        var properties = ToDictionary(state);
        var scopeProperties = new Dictionary<string, object?>(StringComparer.Ordinal);

        getScopeProvider()
            .ForEachScope(
                (scope, stateDictionary) =>
                {
                    foreach (var pair in ToDictionary(scope))
                    {
                        stateDictionary[pair.Key] = pair.Value;
                    }
                },
                scopeProperties
            );

        entries.Enqueue(
            new CapturedLogEntry(
                categoryName,
                logLevel,
                formatter(state, exception),
                properties,
                scopeProperties
            )
        );
    }

    private static IReadOnlyDictionary<string, object?> ToDictionary(object? state)
    {
        if (state is IEnumerable<KeyValuePair<string, object?>> pairs)
        {
            return pairs
                .Where(pair => pair.Key != "{OriginalFormat}")
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        }

        if (state is IEnumerable<KeyValuePair<string, object>> nonNullablePairs)
        {
            return nonNullablePairs.ToDictionary(
                pair => pair.Key,
                pair => (object?)pair.Value,
                StringComparer.Ordinal
            );
        }

        return new Dictionary<string, object?>(StringComparer.Ordinal);
    }
}
