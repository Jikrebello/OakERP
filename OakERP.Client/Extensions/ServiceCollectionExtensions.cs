using Microsoft.Extensions.DependencyInjection;
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
}
