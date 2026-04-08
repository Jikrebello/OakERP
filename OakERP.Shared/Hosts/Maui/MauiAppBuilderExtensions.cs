using OakERP.Client.Configuration;
using OakERP.Common.Exceptions;
using OakERP.Shared.Extensions;

namespace OakERP.Shared.Hosts.Maui;

public static class MauiAppBuilderExtensions
{
    public static MauiAppBuilder AddOakMauiHostServices(this MauiAppBuilder builder)
    {
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

        builder.Services.AddOakMauiHostAdapters();
        builder.Services.AddOakSharedHostServices(apiOptions);
        return builder;
    }
}
