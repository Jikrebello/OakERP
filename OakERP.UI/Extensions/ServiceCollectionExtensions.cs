using Microsoft.Extensions.DependencyInjection;
using OakERP.UI.ViewModels.Auth;

namespace OakERP.UI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOakAuthUiState(this IServiceCollection services)
    {
        services.AddScoped<LoginViewModel>();
        services.AddScoped<RegisterViewModel>();

        return services;
    }
}
