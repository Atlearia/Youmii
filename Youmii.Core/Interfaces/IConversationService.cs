namespace Youmii.Core.Interfaces;

/// <summary>
/// Interface for conversation orchestration.
/// </summary>
public interface IConversationService
{
    /// <summary>
    /// Processes a user message: saves it, extracts facts, builds request payload.
    /// </summary>
    Task<ConversationPrepareResult> PrepareRequestAsync(string userInput);

    /// <summary>
    /// Saves the assistant's response.
    /// </summary>
    Task SaveResponseAsync(string reply);

    /// <summary>
    /// Gets recent conversation history.
    /// </summary>
    Task<IReadOnlyList<Models.ChatMessage>> GetHistoryAsync(int? limit = null);

    /// <summary>
    /// Clears all conversation history.
    /// </summary>
    Task ClearHistoryAsync();
}

/// <summary>
/// Result of preparing a conversation request.
/// </summary>
public sealed class ConversationPrepareResult
{
    public required Models.ChatMessage SavedMessage { get; init; }
    public required Models.BrainRequest Request { get; init; }
}
