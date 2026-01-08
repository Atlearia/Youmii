using Youmii.Core.Interfaces;

namespace Youmii.Features.Chat.Services;

/// <summary>
/// Coordinates chat operations including sending messages and managing conversation state.
/// Extracts chat-related logic from MainViewModel for better separation of concerns.
/// </summary>
public sealed class ChatCoordinator
{
    private readonly IConversationService _conversationService;
    private readonly IBrainClient _brainClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatCoordinator"/> class.
    /// </summary>
    /// <param name="conversationService">The conversation service.</param>
    /// <param name="brainClient">The brain client for AI responses.</param>
    public ChatCoordinator(IConversationService conversationService, IBrainClient brainClient)
    {
        _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
        _brainClient = brainClient ?? throw new ArgumentNullException(nameof(brainClient));
    }

    /// <summary>
    /// Event raised when a message operation starts (for loading state).
    /// </summary>
    public event EventHandler? MessageSending;

    /// <summary>
    /// Event raised when a message operation completes.
    /// </summary>
    public event EventHandler<ChatResponseEventArgs>? MessageReceived;

    /// <summary>
    /// Event raised when a message operation fails.
    /// </summary>
    public event EventHandler<ChatErrorEventArgs>? MessageFailed;

    /// <summary>
    /// Sends a message and gets a response from the brain.
    /// </summary>
    /// <param name="userMessage">The user's message.</param>
    public async Task SendMessageAsync(string userMessage)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
            return;

        MessageSending?.Invoke(this, EventArgs.Empty);

        try
        {
            // Prepare request (saves user message, extracts facts)
            var result = await _conversationService.PrepareRequestAsync(userMessage);

            // Send to brain
            var response = await _brainClient.SendMessageAsync(result.Request);

            // Save response
            await _conversationService.SaveResponseAsync(response.Reply);

            // Raise success event
            MessageReceived?.Invoke(this, new ChatResponseEventArgs(response.Reply));
        }
        catch (Exception ex)
        {
            MessageFailed?.Invoke(this, new ChatErrorEventArgs(ex.Message));
        }
    }

    /// <summary>
    /// Clears the conversation history.
    /// </summary>
    public async Task ClearHistoryAsync()
    {
        await _conversationService.ClearHistoryAsync();
    }
}

/// <summary>
/// Event arguments for chat responses.
/// </summary>
public sealed class ChatResponseEventArgs : EventArgs
{
    public ChatResponseEventArgs(string response)
    {
        Response = response;
    }

    /// <summary>
    /// Gets the response message.
    /// </summary>
    public string Response { get; }
}

/// <summary>
/// Event arguments for chat errors.
/// </summary>
public sealed class ChatErrorEventArgs : EventArgs
{
    public ChatErrorEventArgs(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string ErrorMessage { get; }
}
