using Youmii.Core.Interfaces;
using Youmii.Core.Models;

namespace Youmii.Core.Services;

/// <summary>
/// Orchestrates the conversation flow: saving messages, extracting facts, calling brain.
/// </summary>
public sealed class ConversationService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IFactRepository _factRepository;
    private readonly IFactExtractor _factExtractor;
    private readonly int _maxHistoryMessages;

    public ConversationService(
        IMessageRepository messageRepository,
        IFactRepository factRepository,
        IFactExtractor factExtractor,
        int maxHistoryMessages = 20)
    {
        _messageRepository = messageRepository;
        _factRepository = factRepository;
        _factExtractor = factExtractor;
        _maxHistoryMessages = maxHistoryMessages;
    }

    /// <summary>
    /// Processes a user message: saves it, extracts facts, builds request payload.
    /// </summary>
    public async Task<(ChatMessage savedMessage, BrainRequest request)> PrepareRequestAsync(string userInput)
    {
        // Save user message
        var userMessage = ChatMessage.User(userInput);
        var saved = await _messageRepository.AddMessageAsync(userMessage);

        // Extract and save facts
        var extractedFacts = _factExtractor.ExtractFacts(userInput);
        foreach (var (key, value) in extractedFacts)
        {
            await _factRepository.UpsertFactAsync(key, value);
        }

        // Load history (excluding current message, it's included separately)
        var history = await _messageRepository.GetRecentMessagesAsync(_maxHistoryMessages);
        
        // Load all facts
        var allFacts = await _factRepository.GetAllFactsAsync();

        // Build request
        var request = new BrainRequest
        {
            Message = userInput,
            History = history
                .Where(m => m.Id != saved.Id) // Exclude current message from history
                .Select(BrainHistoryItem.FromChatMessage)
                .ToList(),
            Facts = allFacts.ToDictionary(f => f.Key, f => f.Value)
        };

        return (saved, request);
    }

    /// <summary>
    /// Saves the assistant's response.
    /// </summary>
    public async Task<ChatMessage> SaveResponseAsync(string reply)
    {
        var assistantMessage = ChatMessage.Assistant(reply);
        return await _messageRepository.AddMessageAsync(assistantMessage);
    }

    /// <summary>
    /// Gets recent conversation history.
    /// </summary>
    public Task<IReadOnlyList<ChatMessage>> GetHistoryAsync(int? limit = null)
    {
        return _messageRepository.GetRecentMessagesAsync(limit ?? _maxHistoryMessages);
    }

    /// <summary>
    /// Clears all conversation history.
    /// </summary>
    public Task ClearHistoryAsync()
    {
        return _messageRepository.ClearAsync();
    }

    /// <summary>
    /// Trims history to keep only the most recent N messages.
    /// Returns the number of messages after trimming.
    /// </summary>
    public static IReadOnlyList<ChatMessage> TrimHistory(IReadOnlyList<ChatMessage> messages, int maxCount)
    {
        if (messages.Count <= maxCount)
            return messages;

        return messages
            .OrderByDescending(m => m.CreatedAt)
            .Take(maxCount)
            .OrderBy(m => m.CreatedAt)
            .ToList();
    }
}
