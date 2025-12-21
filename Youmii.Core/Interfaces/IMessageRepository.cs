using Youmii.Core.Models;

namespace Youmii.Core.Interfaces;

/// <summary>
/// Repository for storing and retrieving chat messages.
/// </summary>
public interface IMessageRepository
{
    /// <summary>
    /// Adds a new message to the history.
    /// </summary>
    Task<ChatMessage> AddMessageAsync(ChatMessage message);

    /// <summary>
    /// Gets the most recent messages, ordered from oldest to newest.
    /// </summary>
    Task<IReadOnlyList<ChatMessage>> GetRecentMessagesAsync(int limit);

    /// <summary>
    /// Clears all message history.
    /// </summary>
    Task ClearAsync();
}
