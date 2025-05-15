using Microsoft.Extensions.DependencyInjection;
using OakERP.Shared.Services.Api;
using OakERP.Shared.Services.Auth;
using OakERP.Shared.ViewModels.Auth;

namespace OakERP.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOakClientServices(this IServiceCollection services)
    {
        // API Infrastructure
        services.AddScoped<IApiClient, ApiClient>();

        // Auth
        services.AddScoped<IAuthService, AuthService>();

        // ViewModels
        services.AddScoped<LoginViewModel>();

        return services;
    }
}
