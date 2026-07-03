using Microsoft.Extensions.DependencyInjection;
using OakERP.Client.Services.Errors;
using OakERP.UI.Errors;
using OakERP.UI.ViewModels.Auth;
using OakERP.UI.ViewModels.Support;

namespace OakERP.UI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOakAuthUiState(this IServiceCollection services)
    {
        services.AddScoped<IUiErrorNotifier, FluentToastErrorNotifier>();
        services.AddScoped<IClientErrorHandler, ToastClientErrorHandler>();
        services.AddScoped<IUiOperationRunner, UiOperationRunner>();
        services.AddScoped<LoginViewModel>();
        services.AddScoped<RegisterViewModel>();

        return services;
    }
}
