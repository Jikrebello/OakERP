using System.Net;

namespace OakERP.Common.Exceptions;

public sealed class ConfigurationValidationException(
    string settingKey,
    string message,
    Exception? innerException = null
) : OakErpException(
    message,
    HttpStatusCode.InternalServerError,
    "Application configuration is invalid.",
    innerException
)
{
    public string SettingKey { get; } = settingKey;
}
