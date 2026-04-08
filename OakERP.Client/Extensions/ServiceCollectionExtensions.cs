using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OakERP.Client.Services.Api;
using OakERP.Client.Services.Auth;
using OakERP.Common.Abstractions;

namespace OakERP.Client.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOakClientCoreServices(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuthSessionManager, AuthSessionManager>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }

    public static IServiceCollection AddOakApiClient(
        this IServiceCollection services,
        ApiClientOptions options
    )
    {
        ArgumentNullException.ThrowIfNull(options);

        Uri baseUri = options.GetBaseUri();

        services.AddSingleton(options);
        services.AddScoped<AuthTokenHandler>();
        services.AddScoped<IApiClient>(sp =>
        {
            var tokenHandler = sp.GetRequiredService<AuthTokenHandler>();
            tokenHandler.InnerHandler = new HttpClientHandler();

            var client = new HttpClient(tokenHandler) { BaseAddress = baseUri };
            var logger = sp.GetRequiredService<ILogger<ApiClient>>();
            return new ApiClient(client, logger);
        });

        return services;
    }
}
