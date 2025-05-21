using OakERP.Common.Abstractions;

namespace OakERP.Services;

/// <summary>
/// Provides platform-specific information for a .NET MAUI application.
/// </summary>
/// <remarks>This service indicates whether the application is running in a hybrid environment.</remarks>
internal class MauiPlatformService : IPlatformService
{
    public bool IsWeb => false;

    public bool IsHybrid => true;
}