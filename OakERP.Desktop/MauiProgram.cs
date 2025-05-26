using Microsoft.Extensions.Logging;
using Microsoft.FluentUI.AspNetCore.Components;
using OakERP.Common.Abstractions;
using OakERP.Services;
using OakERP.Shared.Extensions;
using OakERP.Shared.Services;
using OakERP.Shared.Services.Api;

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

        // Device-specific services used by shared Razor UI
        builder.Services.AddSingleton<IFormFactor, FormFactor>();
        builder.Services.AddScoped<ITokenStore, MauiTokenStore>();
        builder.Services.AddScoped<IPlatformService, MauiPlatformService>();

        builder.Services.AddScoped<AuthTokenHandler>();

        builder.Services.AddScoped<IApiClient>(sp =>
        {
            var tokenHandler = sp.GetRequiredService<AuthTokenHandler>();
            tokenHandler.InnerHandler = new HttpClientHandler();

            var client = new HttpClient(tokenHandler)
            {
                BaseAddress = new Uri("https://localhost:7057/api/"),
            };
            return new ApiClient(client);
        });

        // Shared Razor Class Lib services
        builder.Services.AddOakClientServices();

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