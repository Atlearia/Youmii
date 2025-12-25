namespace Youmii.Core.Interfaces;

/// <summary>
/// Initializes the database schema.
/// </summary>
public interface IDatabaseInitializer
{
    /// <summary>
    /// Initializes the database schema if it doesn't exist.
    /// </summary>
    Task InitializeAsync();
}
