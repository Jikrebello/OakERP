using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OakERP.Auth;
using OakERP.Common.DTOs.Auth;
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

        var body = await ReadJsonAsync(response);
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

        var body = await ReadJsonAsync(response);
        body.GetProperty("status").GetInt32().ShouldBe(
            (int)HttpStatusCode.InternalServerError
        );
        body.GetProperty("title").GetString().ShouldBe("An unexpected error occurred.");
        body.GetProperty("correlationId").GetString().ShouldBe(correlationId);
        body.GetProperty("traceId").GetString().ShouldNotBeNullOrWhiteSpace();
        body.ToString().ShouldNotContain("Simulated login failure");
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.Clone();
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
