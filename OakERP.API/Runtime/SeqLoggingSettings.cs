using OakERP.Common.Exceptions;

namespace OakERP.API.Runtime;

public sealed class SeqLoggingSettings
{
    public const string SectionName = "Serilog:Seq";

    public bool Enabled { get; init; }

    public string? ServerUrl { get; init; }

    public string? ApiKey { get; init; }

    public static SeqLoggingSettings Bind(IConfiguration configuration)
    {
        var settings =
            configuration.GetSection(SectionName).Get<SeqLoggingSettings>()
            ?? new SeqLoggingSettings();

        if (settings.Enabled && string.IsNullOrWhiteSpace(settings.ServerUrl))
        {
            throw new ConfigurationValidationException(
                SectionName + ":ServerUrl",
                $"{SectionName}:ServerUrl must be configured when Seq logging is enabled."
            );
        }

        return settings;
    }

    public string? GetApiKeyOrNull()
    {
        return string.IsNullOrWhiteSpace(ApiKey) ? null : ApiKey;
    }
}
