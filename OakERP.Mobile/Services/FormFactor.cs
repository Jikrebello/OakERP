using OakERP.Shared.Services;

namespace OakERP.Mobile.Services;

public class FormFactor : IFormFactor
{
    public string GetFormFactor() => DeviceInfo.Idiom.ToString();

    public string GetPlatform() => DeviceInfo.Platform + " - " + DeviceInfo.VersionString;
}
