using Microsoft.Extensions.DependencyInjection;
using OakERP.Client.Configuration;
using OakERP.Client.Extensions;
using OakERP.UI.Extensions;

namespace OakERP.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOakSharedHostServices(
        this IServiceCollection services,
        ApiClientOptions apiOptions
    )
    {
        services.AddOakClientCoreServices();
        services.AddOakAuthUiState();
        services.AddOakApiClient(apiOptions);
        return services;
    }
}
