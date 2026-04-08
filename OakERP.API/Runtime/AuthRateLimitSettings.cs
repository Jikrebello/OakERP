using OakERP.Common.Exceptions;

namespace OakERP.API.Runtime;

public sealed class AuthRateLimitSettings
{
    public const string SectionName = "RuntimeSupport:RateLimiting:Auth";
    public const string PolicyName = "auth-fixed-window";
    public const string UnknownClientPartition = "unknown-client";

    public int PermitLimit { get; init; } = 10;

    public int WindowSeconds { get; init; } = 60;

    public int QueueLimit { get; init; } = 0;

    public static AuthRateLimitSettings Bind(IConfiguration configuration)
    {
        var settings =
            configuration.GetSection(SectionName).Get<AuthRateLimitSettings>()
            ?? new AuthRateLimitSettings();

        if (settings.PermitLimit <= 0)
        {
            throw new ConfigurationValidationException(
                SectionName + ":PermitLimit",
                $"{SectionName}:PermitLimit must be greater than 0."
            );
        }

        if (settings.WindowSeconds <= 0)
        {
            throw new ConfigurationValidationException(
                SectionName + ":WindowSeconds",
                $"{SectionName}:WindowSeconds must be greater than 0."
            );
        }

        if (settings.QueueLimit != 0)
        {
            throw new ConfigurationValidationException(
                SectionName + ":QueueLimit",
                $"{SectionName}:QueueLimit must be 0."
            );
        }

        return settings;
    }
}
