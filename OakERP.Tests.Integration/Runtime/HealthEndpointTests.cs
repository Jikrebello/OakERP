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
