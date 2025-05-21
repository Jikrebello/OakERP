using Microsoft.Extensions.Logging;
using Microsoft.FluentUI.AspNetCore.Components;
using OakERP.Common.Abstractions;
using OakERP.Services;
using OakERP.Shared.Extensions;
using OakERP.Shared.Services;
using OakERP.Shared.Services.Auth;

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
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        builder.Services.AddScoped<IPlatformService, MauiPlatformService>();

        builder.Services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5001"),
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