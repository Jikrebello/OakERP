using Microsoft.Extensions.DependencyInjection;
using OakERP.Common.Abstractions;
using OakERP.Shared.Services.Api;
using OakERP.Shared.Services.Auth;
using OakERP.Shared.ViewModels.Auth;

namespace OakERP.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Oak client services, including authentication, API infrastructure, and view models, into the specified
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>This method adds the following services to the dependency injection container: <list
    /// type="bullet"> <item><description>Authentication services, including <see cref="ICurrentUserService"/> and <see
    /// cref="IAuthSessionManager"/>.</description></item> <item><description>API infrastructure, including the <see
    /// cref="ApiClient"/>.</description></item> <item><description>Application services, such as <see
    /// cref="IAuthService"/>.</description></item> <item><description>View models, including <see
    /// cref="LoginViewModel"/> and <see cref="RegisterViewModel"/>.</description></item> </list></remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the services will be added.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
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
