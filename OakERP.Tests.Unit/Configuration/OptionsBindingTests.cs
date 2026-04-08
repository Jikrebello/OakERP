using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OakERP.Auth.Extensions;
using OakERP.Client.Configuration;
using OakERP.Client.Extensions;
using OakERP.Client.Services.Api;
using OakERP.Common.Abstractions;
using Shouldly;

namespace OakERP.Tests.Unit.Configuration;

public sealed class OptionsBindingTests
{
    [Fact]
    public void AddJwtAuth_Should_Bind_Validated_JwtOptions()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    [JwtOptions.SectionName + ":Key"] = "01234567890123456789012345678901",
                    [JwtOptions.SectionName + ":Issuer"] = "oak-issuer",
                    [JwtOptions.SectionName + ":Audience"] = "oak-audience",
                    [JwtOptions.SectionName + ":ExpireMinutes"] = "90",
                }
            )
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddJwtAuth(configuration);
        services.AddAuthServices();

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        JwtOptions options = serviceProvider.GetRequiredService<IOptions<JwtOptions>>().Value;
        options.Issuer.ShouldBe("oak-issuer");
        options.Audience.ShouldBe("oak-audience");
        options.ExpireMinutes.ShouldBe(90);

        var generator = serviceProvider.GetRequiredService<IJwtGenerator>();
        string token = generator.Generate(
            new JwtTokenInput("user-1", "user@example.com", Guid.NewGuid())
        );

        token.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Client_Core_Registration_Should_Expose_Api_And_Auth_Services()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<ITokenStore, FakeTokenStore>();
        services.AddOakClientCoreServices();
        services.AddOakApiClient(new ApiClientOptions { BaseUrl = "https://example.test/api/" });

        services.ShouldContain(x => x.ServiceType == typeof(IApiClient));
        services.ShouldContain(x =>
            x.ServiceType == typeof(OakERP.Client.Services.Auth.IAuthSessionManager)
        );
        services.ShouldContain(x => x.ServiceType == typeof(ICurrentUserService));
    }

    private sealed class FakeTokenStore : ITokenStore
    {
        public Task SaveTokenAsync(string token) => Task.CompletedTask;

        public Task<string?> GetTokenAsync() => Task.FromResult<string?>(null);

        public Task DeleteTokenAsync() => Task.CompletedTask;
    }
}
