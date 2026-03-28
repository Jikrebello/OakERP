using Microsoft.Extensions.DependencyInjection;
using OakERP.Client.Extensions;
using OakERP.UI.Extensions;

namespace OakERP.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers shared Oak UI services and the required client core services into the specified
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>This method adds the following services to the dependency injection container: <list
    /// type="bullet"> <item><description>Client core services required by shared UI.</description></item>
    /// <item><description>Auth UI state required by the shared auth pages.</description></item> </list></remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the services will be added.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddOakClientServices(this IServiceCollection services)
    {
        services.AddOakClientCoreServices();
        services.AddOakAuthUiState();

        return services;
    }
}
