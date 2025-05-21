using OakERP.Common.Abstractions;

namespace OakERP.Web.Services;

/// <summary>
/// Provides platform-specific information for a Blazor application.
/// </summary>
/// <remarks>This service identifies the platform on which the Blazor application is running. It is designed to
/// distinguish between web-based and hybrid environments.</remarks>
internal class BlazorPlatformService : IPlatformService
{
    public bool IsWeb => true;

    public bool IsHybrid => false;
}