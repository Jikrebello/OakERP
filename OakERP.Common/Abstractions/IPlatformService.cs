namespace OakERP.Common.Abstractions;

/// <summary>
/// Provides platform-specific information about the current application environment.
/// </summary>
/// <remarks>This interface is used to determine the type of platform the application is running on,  such as
/// whether it is a web-based or hybrid environment. Implementations of this interface  should provide the appropriate
/// platform-specific logic to determine these values.</remarks>
public interface IPlatformService
{
    /// <summary>
    /// Gets a value indicating whether the current environment is a web environment.
    /// </summary>
    bool IsWeb { get; }

    /// <summary>
    /// Gets a value indicating whether the current configuration is a hybrid setup.
    /// </summary>
    bool IsHybrid { get; }
}