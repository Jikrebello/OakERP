using Microsoft.Extensions.DependencyInjection;
using OakERP.Common.Abstractions;
using OakERP.Shared.Services.Api;
using OakERP.Shared.Services.Auth;
using OakERP.Shared.ViewModels.Auth;

namespace OakERP.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOakClientServices(this IServiceCollection services)
    {
        // Auth
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuthSessionManager, AuthSessionManager>();

        // API Infrastructure
        services.AddScoped<ApiClient>();

        // Services
        services.AddScoped<IAuthService, AuthService>();

        // ViewModels
        services.AddScoped<LoginViewModel>();
        services.AddScoped<RegisterViewModel>();

        return services;
    }
}