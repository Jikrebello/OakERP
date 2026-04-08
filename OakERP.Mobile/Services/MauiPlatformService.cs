using OakERP.Common.Abstractions;

namespace OakERP.Mobile.Services;

internal class MauiPlatformService : IPlatformService
{
    public bool IsWeb => false;

    public bool IsHybrid => true;
}
