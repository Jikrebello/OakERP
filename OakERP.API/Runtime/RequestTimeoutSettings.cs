namespace OakERP.API.Runtime;

public sealed class RequestTimeoutSettings
{
    public const string SectionName = "RuntimeSupport:RequestTimeouts";

    public int ControllerSeconds { get; init; } = 30;

    public static RequestTimeoutSettings Bind(IConfiguration configuration)
    {
        var settings = configuration.GetSection(SectionName).Get<RequestTimeoutSettings>()
            ?? new RequestTimeoutSettings();

        if (settings.ControllerSeconds <= 0)
        {
            throw new InvalidOperationException(
                $"{SectionName}:ControllerSeconds must be greater than 0."
            );
        }

        return settings;
    }
}
