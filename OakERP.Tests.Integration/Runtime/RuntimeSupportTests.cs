using System.Net;
using System.Net.Http.Json;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using OakERP.API.Extensions;
using OakERP.Auth;
using OakERP.Common.DTOs.Auth;
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
                new LoginDTO { Email = "missing@example.com", Password = "bad-password" }
            ),
        };

        request.Headers.Add(CorrelationHeaderName, correlationId);

        var response = await Client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
        response.Headers.GetValues(CorrelationHeaderName).Single().ShouldBe(correlationId);

        var result = await response.Content.ReadFromJsonAsync<AuthResultDTO>();
        result.ShouldNotBeNull();
        result.Success.ShouldBeFalse();
        result.Message.ShouldBe("Invalid login credentials.");
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
                new LoginDTO { Email = "boom@example.com", Password = "ignored" }
            ),
        };

        request.Headers.Add(CorrelationHeaderName, correlationId);

        var response = await client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        response.Headers.GetValues(CorrelationHeaderName).Single().ShouldBe(correlationId);

        var body = await RuntimeSupportTestJson.ReadJsonAsync(response);
        body.GetProperty("status").GetInt32().ShouldBe(
            (int)HttpStatusCode.InternalServerError
        );
        body.GetProperty("title").GetString().ShouldBe("An unexpected error occurred.");
        body.GetProperty("correlationId").GetString().ShouldBe(correlationId);
        body.GetProperty("traceId").GetString().ShouldNotBeNullOrWhiteSpace();
        body.ToString().ShouldNotContain("Simulated login failure");
    }
    private sealed class ThrowingAuthService : IAuthService
    {
        public Task<AuthResultDTO> RegisterAsync(RegisterDTO dto)
        {
            throw new InvalidOperationException("Simulated registration failure");
        }

        public Task<AuthResultDTO> LoginAsync(LoginDTO dto)
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

        var liveEndpoint = routeEndpoints.Single(endpoint => endpoint.RoutePattern.RawText == "/health/live");
        var readyEndpoint = routeEndpoints.Single(endpoint => endpoint.RoutePattern.RawText == "/health/ready");

        liveEndpoint.Metadata.Any(metadata => metadata.GetType().Name.Contains("DisableRequestTimeout"))
            .ShouldBeTrue();
        readyEndpoint.Metadata.Any(metadata => metadata.GetType().Name.Contains("DisableRequestTimeout"))
            .ShouldBeTrue();
    }

    [Test]
    public async Task Timeout_Response_Writer_Should_Write_ProblemDetails_With_Correlation_Id_And_TraceId()
    {
        await using var factory = new OakErpWebFactory();

        var timeoutOptions = factory.Services.GetRequiredService<IOptions<RequestTimeoutOptions>>().Value;
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
        using var document = JsonDocument.Parse(await new StreamReader(context.Response.Body).ReadToEndAsync());
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

internal static class RuntimeSupportTestJson
{
    public static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.Clone();
    }
}
