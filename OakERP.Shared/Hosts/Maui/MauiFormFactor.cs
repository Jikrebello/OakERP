using OakERP.Shared.Services;

namespace OakERP.Shared.Hosts.Maui;

public sealed class MauiFormFactor : IFormFactor
{
    public string GetFormFactor() => DeviceInfo.Idiom.ToString();

    public string GetPlatform() => DeviceInfo.Platform + " - " + DeviceInfo.VersionString;
}
