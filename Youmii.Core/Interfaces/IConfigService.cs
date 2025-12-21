using Youmii.Core.Models;

namespace Youmii.Core.Interfaces;

/// <summary>
/// Provides application configuration.
/// </summary>
public interface IConfigService
{
    /// <summary>
    /// Gets the application settings.
    /// </summary>
    AppSettings Settings { get; }
}
