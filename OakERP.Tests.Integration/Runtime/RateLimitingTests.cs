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
