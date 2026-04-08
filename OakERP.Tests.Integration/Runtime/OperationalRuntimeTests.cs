using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using OakERP.API.Extensions;
using OakERP.Infrastructure.Persistence;
using Shouldly;

namespace OakERP.Tests.Integration.Runtime;

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
