using System.Collections.Concurrent;
using System.IO;
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
using OakERP.Auth;
using OakERP.Common.Dtos.Auth;
using OakERP.Infrastructure.Persistence;
using OakERP.Tests.Integration.TestSetup;
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
            .Entries.Where(entry => entry.Category == "OakERP.Auth.AuthService")
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
            .Entries.Where(entry => entry.Category == "OakERP.Auth.AuthService")
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
}

[TestFixture]
public class HealthEndpointTests : WebApiIntegrationTestBase
{
    [Test]
    public async Task Live_Endpoint_Should_Return_Ok_Without_Database_Dependency()
    {
        var response = await Client.GetAsync("/health/live");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task Ready_Endpoint_Should_Return_Ok_When_Database_Is_Reachable()
    {
        var response = await Client.GetAsync("/health/ready");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}

[TestFixture]
public class OperationalRuntimeTests
{
    [Test]
    public async Task Live_Endpoint_Should_Stay_Healthy_When_Database_Config_Is_Broken()
    {
        await using var factory = CreateFactoryWithBrokenDatabase();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task Ready_Endpoint_Should_Return_ServiceUnavailable_When_Database_Config_Is_Broken()
    {
        await using var factory = CreateFactoryWithBrokenDatabase();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health/ready");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Test]
    public async Task Health_Endpoints_Should_Disable_RequestTimeout()
    {
        await using var factory = new OakErpWebFactory();

        var endpoints = factory.Services.GetRequiredService<EndpointDataSource>().Endpoints;
        var routeEndpoints = endpoints.OfType<RouteEndpoint>().ToList();

        var liveEndpoint = routeEndpoints.Single(endpoint =>
            endpoint.RoutePattern.RawText == "/health/live"
        );
        var readyEndpoint = routeEndpoints.Single(endpoint =>
            endpoint.RoutePattern.RawText == "/health/ready"
        );

        liveEndpoint
            .Metadata.Any(metadata => metadata.GetType().Name.Contains("DisableRequestTimeout"))
            .ShouldBeTrue();
        readyEndpoint
            .Metadata.Any(metadata => metadata.GetType().Name.Contains("DisableRequestTimeout"))
            .ShouldBeTrue();
    }

    [Test]
    public async Task Timeout_Response_Writer_Should_Write_ProblemDetails_With_Correlation_Id_And_TraceId()
    {
        await using var factory = new OakErpWebFactory();

        var timeoutOptions = factory
            .Services.GetRequiredService<IOptions<RequestTimeoutOptions>>()
            .Value;
        timeoutOptions.DefaultPolicy.ShouldNotBeNull();
        timeoutOptions.DefaultPolicy!.WriteTimeoutResponse.ShouldNotBeNull();

        var context = new DefaultHttpContext { RequestServices = factory.Services };
        context.TraceIdentifier = "timeout-trace-id";
        context.Request.Method = HttpMethod.Get.Method;
        context.Request.Path = "/api/auth/login";
        context.Request.Headers.Accept = "application/json";
        context.Response.Body = new MemoryStream();
        context.SetCorrelationId("timeout-correlation-id");

        await timeoutOptions.DefaultPolicy.WriteTimeoutResponse!(context);

        context.Response.StatusCode.ShouldBe(StatusCodes.Status503ServiceUnavailable);
        context.Response.ContentType.ShouldBe("application/problem+json");

        context.Response.Body.Position = 0;
        using var document = JsonDocument.Parse(
            await new StreamReader(context.Response.Body).ReadToEndAsync()
        );
        var body = document.RootElement;

        body.GetProperty("status").GetInt32().ShouldBe(StatusCodes.Status503ServiceUnavailable);
        body.GetProperty("title").GetString().ShouldBe("The request timed out.");
        body.GetProperty("correlationId").GetString().ShouldBe("timeout-correlation-id");
        body.GetProperty("traceId").GetString().ShouldBe("timeout-trace-id");
    }

    private static WebApplicationFactory<Program> CreateFactoryWithBrokenDatabase()
    {
        return new OakErpWebFactory().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ApplicationDbContext>();
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseNpgsql(
                        "Host=127.0.0.1;Port=1;Database=oakerp_bad;Username=oakadmin;Password=oakpass;Timeout=1;Command Timeout=1"
                    );
                });
            });
        });
    }
}

[TestFixture]
public class RateLimitingTests : WebApiIntegrationTestBase
{
    private const string CorrelationHeaderName = "X-Correlation-ID";

    [Test]
    public async Task Login_Should_Preserve_Dto_Body_Before_Rate_Limit_Is_Hit()
    {
        var response = await SendInvalidLoginAsync(Client);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");

        var result = await response.Content.ReadFromJsonAsync<AuthResultDto>();
        result.ShouldNotBeNull();
        result.Success.ShouldBeFalse();
        result.Message.ShouldBe("Invalid login credentials.");
    }

    [Test]
    public async Task Throttled_Login_Should_Return_ProblemDetails_And_Echo_Correlation_Id()
    {
        const string correlationId = "rate-limit-correlation-id";
        await ExhaustLoginRateLimitAsync(Client);

        using var throttledRequest = CreateInvalidLoginRequest();
        throttledRequest.Headers.Add(CorrelationHeaderName, correlationId);

        var response = await Client.SendAsync(throttledRequest);

        response.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        response.Headers.GetValues(CorrelationHeaderName).Single().ShouldBe(correlationId);

        var body = await RuntimeSupportTestJson.ReadJsonAsync(response);
        body.GetProperty("status").GetInt32().ShouldBe((int)HttpStatusCode.TooManyRequests);
        body.GetProperty("title").GetString().ShouldBe("Too many requests.");
        body.GetProperty("correlationId").GetString().ShouldBe(correlationId);
        body.GetProperty("traceId").GetString().ShouldNotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task Login_Exhaustion_Should_Not_Throttle_Register()
    {
        await ExhaustLoginRateLimitAsync(Client);
        (await SendInvalidLoginAsync(Client)).StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);

        using var registerRequest = new HttpRequestMessage(HttpMethod.Post, ApiRoutes.Auth.Register)
        {
            Content = JsonContent.Create(
                new RegisterDto
                {
                    Email = $"ratelimit_{TestId}@oak.test",
                    Password = "TestPass123!",
                    ConfirmPassword = "MismatchPass123!",
                    FirstName = "Rate",
                    LastName = "Limit",
                    PhoneNumber = "123456789",
                    TenantName = $"RateLimitTenant_{TestId}",
                }
            ),
        };

        var response = await Client.SendAsync(registerRequest);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");

        var result = await response.Content.ReadFromJsonAsync<AuthResultDto>();
        result.ShouldNotBeNull();
        result.Success.ShouldBeFalse();
        result.Message.ShouldContain("Passwords do not match");
    }

    [Test]
    public async Task Health_Endpoints_Should_Not_Be_Affected_By_Auth_Rate_Limit_Exhaustion()
    {
        await ExhaustLoginRateLimitAsync(Client);
        (await SendInvalidLoginAsync(Client)).StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);

        var liveResponse = await Client.GetAsync("/health/live");
        var readyResponse = await Client.GetAsync("/health/ready");

        liveResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        readyResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    private async Task ExhaustLoginRateLimitAsync(HttpClient client)
    {
        for (var attempt = 0; attempt < GetConfiguredAuthPermitLimit(); attempt++)
        {
            var response = await SendInvalidLoginAsync(client);
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }
    }

    private static HttpRequestMessage CreateInvalidLoginRequest()
    {
        return new HttpRequestMessage(HttpMethod.Post, ApiRoutes.Auth.Login)
        {
            Content = JsonContent.Create(
                new LoginDto { Email = "missing@example.com", Password = "bad-password" }
            ),
        };
    }

    private static async Task<HttpResponseMessage> SendInvalidLoginAsync(HttpClient client)
    {
        using var request = CreateInvalidLoginRequest();
        return await client.SendAsync(request);
    }

    private int GetConfiguredAuthPermitLimit()
    {
        return Factory
            .Services.GetRequiredService<IConfiguration>()
            .GetValue<int>($"{AuthRateLimitSettings.SectionName}:PermitLimit");
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
