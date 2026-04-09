using OakERP.Common.Abstractions;
using OakERP.Shared.Services;

namespace OakERP.Shared.Hosts.Maui;

public static class MauiHostServiceCollectionExtensions
{
    public static IServiceCollection AddOakMauiHostAdapters(this IServiceCollection services)
    {
        services.AddSingleton<IFormFactor, MauiFormFactor>();
        services.AddScoped<ITokenStore, MauiTokenStore>();
        services.AddScoped<IPlatformService, MauiPlatformService>();
        return services;
    }
}
