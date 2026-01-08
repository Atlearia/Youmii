namespace Youmii.Core.Interfaces;

/// <summary>
/// Service that provides random idle messages when the user hasn't interacted with the character.
/// </summary>
public interface IIdleMessageService
{
    /// <summary>
    /// Gets a random idle message from the predefined collection.
    /// </summary>
    string GetRandomMessage();

    /// <summary>
    /// Gets the minimum idle interval in seconds.
    /// </summary>
    int MinIntervalSeconds { get; }

    /// <summary>
    /// Gets the maximum idle interval in seconds.
    /// </summary>
    int MaxIntervalSeconds { get; }

    /// <summary>
    /// Generates a random interval between min and max seconds.
    /// </summary>
    TimeSpan GetRandomInterval();
}
