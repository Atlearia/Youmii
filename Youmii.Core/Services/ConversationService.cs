using Youmii.Core.Interfaces;
using Youmii.Core.Models;

namespace Youmii.Core.Services;

/// <summary>
/// Orchestrates the conversation flow: saving messages, extracting facts, calling brain.
/// </summary>
public sealed class ConversationService : IConversationService
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
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        _factRepository = factRepository ?? throw new ArgumentNullException(nameof(factRepository));
        _factExtractor = factExtractor ?? throw new ArgumentNullException(nameof(factExtractor));
        _maxHistoryMessages = maxHistoryMessages;
    }

    /// <inheritdoc />
    public async Task<ConversationPrepareResult> PrepareRequestAsync(string userInput)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userInput);

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
                .Where(m => m.Id != saved.Id)
                .Select(BrainHistoryItem.FromChatMessage)
                .ToList(),
            Facts = allFacts.ToDictionary(f => f.Key, f => f.Value)
        };

        return new ConversationPrepareResult
        {
            SavedMessage = saved,
            Request = request
        };
    }

    /// <inheritdoc />
    public async Task SaveResponseAsync(string reply)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reply);
        var assistantMessage = ChatMessage.Assistant(reply);
        await _messageRepository.AddMessageAsync(assistantMessage);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ChatMessage>> GetHistoryAsync(int? limit = null)
    {
        return _messageRepository.GetRecentMessagesAsync(limit ?? _maxHistoryMessages);
    }

    /// <inheritdoc />
    public Task ClearHistoryAsync()
    {
        return _messageRepository.ClearAsync();
    }
}
