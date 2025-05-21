using OakERP.Common.Abstractions;

namespace OakERP.Services;

internal class MauiPlatformService : IPlatformService
{
    public bool IsWeb => false;

    public bool IsHybrid => true;
}