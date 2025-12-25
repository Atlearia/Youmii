namespace Youmii.Core.DependencyInjection;

/// <summary>
/// Handles application lifecycle events.
/// </summary>
public interface IApplicationLifetime
{
    /// <summary>
    /// Initializes all services asynchronously.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Shuts down all services and releases resources.
    /// </summary>
    Task ShutdownAsync();
}
