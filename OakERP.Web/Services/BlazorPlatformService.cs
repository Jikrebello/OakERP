using OakERP.Common.Abstractions;

namespace OakERP.Web.Services;

internal class BlazorPlatformService : IPlatformService
{
    public bool IsWeb => true;

    public bool IsHybrid => false;
}