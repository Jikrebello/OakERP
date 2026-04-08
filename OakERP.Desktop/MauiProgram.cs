using Microsoft.Extensions.Logging;
using Microsoft.FluentUI.AspNetCore.Components;
using OakERP.Client;
using OakERP.Client.Extensions;
using OakERP.Client.Services.Api;
using OakERP.Common.Abstractions;
using OakERP.Services;
using OakERP.Shared.Services;
using OakERP.UI.Extensions;

namespace OakERP;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        var apiOptions = new ApiClientOptions
        {
            BaseUrl =
                builder.Configuration["Api:BaseUrl"]
                ?? Environment.GetEnvironmentVariable("OakERP__Api__BaseUrl")
                ?? throw new InvalidOperationException("Api:BaseUrl is not configured."),
        };

        // Device-specific services used by shared Razor UI
        builder.Services.AddSingleton<IFormFactor, FormFactor>();
        builder.Services.AddScoped<ITokenStore, MauiTokenStore>();
        builder.Services.AddScoped<IPlatformService, MauiPlatformService>();

        // Shared Razor Class Lib services
        builder.Services.AddOakClientCoreServices();
        builder.Services.AddOakAuthUiState();
        builder.Services.AddOakApiClient(apiOptions);

        // UI setup
        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddFluentUIComponents();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
