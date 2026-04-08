using OakERP.Common.Abstractions;

namespace OakERP.Shared.Hosts.Maui;

internal sealed class MauiPlatformService : IPlatformService
{
    public bool IsWeb => false;

    public bool IsHybrid => true;
}
