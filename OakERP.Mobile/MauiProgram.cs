using Microsoft.Extensions.Logging;
using Microsoft.FluentUI.AspNetCore.Components;
using OakERP.Client.Configuration;
using OakERP.Common.Abstractions;
using OakERP.Common.Exceptions;
using OakERP.Mobile.Services;
using OakERP.Shared.Extensions;
using OakERP.Shared.Services;

namespace OakERP.Mobile;

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

        var apiBaseUrl =
            builder.Configuration["Api:BaseUrl"]
            ?? Environment.GetEnvironmentVariable("OakERP__Api__BaseUrl");

        if (string.IsNullOrWhiteSpace(apiBaseUrl))
        {
            throw new ConfigurationValidationException(
                "Api:BaseUrl",
                "Api:BaseUrl is not configured."
            );
        }

        var apiOptions = new ApiClientOptions { BaseUrl = apiBaseUrl };
        apiOptions.GetBaseUri();

        builder.Services.AddSingleton<IFormFactor, FormFactor>();
        builder.Services.AddScoped<ITokenStore, MauiTokenStore>();
        builder.Services.AddScoped<IPlatformService, MauiPlatformService>();
        builder.Services.AddOakSharedHostServices(apiOptions);
        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddFluentUIComponents();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
