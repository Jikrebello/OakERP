using System.Net;
using NUnit.Framework;
using Shouldly;

namespace OakERP.Tests.Integration.Runtime;

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
