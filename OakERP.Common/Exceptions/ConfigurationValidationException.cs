using OakERP.Common.Errors;

namespace OakERP.Common.Exceptions;

public sealed class ConfigurationValidationException(
    string settingKey,
    string message,
    Exception? innerException = null
) : OakErpException(
    message,
    FailureKind.Unexpected,
    "Application configuration is invalid.",
    innerException
)
{
    public string SettingKey { get; } = settingKey;
}
