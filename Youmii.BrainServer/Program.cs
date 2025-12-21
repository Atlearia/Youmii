using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

var app = builder.Build();

// Health check endpoint
app.MapGet("/", () => "Youmii Brain Server is running!");

// Chat endpoint
app.MapPost("/chat", (ChatRequest request) =>
{
    var reply = GenerateReply(request);
    return Results.Ok(new ChatResponse { Reply = reply });
});

app.Run();

static string GenerateReply(ChatRequest request)
{
    var message = request.Message.ToLowerInvariant();
    
    // Check if we know the user's name
    string? userName = null;
    if (request.Facts.TryGetValue("name", out var name))
    {
        userName = name;
    }

    // Generate response based on message content
    if (message.Contains("hello") || message.Contains("hi"))
    {
        return userName != null
            ? $"Hello, {userName}! Great to see you! How can I help you today?"
            : "Hello there! What's your name?";
    }

    if (message.Contains("how are you"))
    {
        return userName != null
            ? $"I'm doing wonderfully, {userName}! Thanks for asking. How about you?"
            : "I'm doing great! Thanks for asking!";
    }

    if (message.Contains("bye") || message.Contains("goodbye"))
    {
        return userName != null
            ? $"Goodbye, {userName}! It was nice chatting with you. See you next time!"
            : "Goodbye! Come back soon!";
    }

    if (message.Contains("what") && message.Contains("name"))
    {
        return "I'm Youmii, your friendly desktop companion!";
    }

    if (message.Contains("help"))
    {
        return "I'm here to keep you company! You can tell me about yourself, " +
               "ask me questions, or just chat. Try saying 'My name is [your name]'!";
    }

    if (message.Contains("thank"))
    {
        return userName != null
            ? $"You're welcome, {userName}! Happy to help!"
            : "You're welcome! Anytime!";
    }

    // Check conversation history for context
    if (request.History.Count > 0)
    {
        var lastAssistant = request.History.LastOrDefault(h => h.Role == "assistant");
        if (lastAssistant?.Content.Contains("name") == true && !string.IsNullOrEmpty(userName))
        {
            return $"Nice to meet you, {userName}! I'll remember that. What would you like to talk about?";
        }
    }

    // Default responses
    var responses = new[]
    {
        "That's interesting! Tell me more.",
        "I see! What else is on your mind?",
        "Hmm, I'm thinking about that...",
        userName != null ? $"Thanks for sharing, {userName}!" : "Thanks for sharing that with me!",
        "I appreciate you telling me that!",
        "That's a great point to consider!",
    };

    var random = new Random();
    return responses[random.Next(responses.Length)];
}

// Request/Response models
public sealed class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public List<HistoryItem> History { get; set; } = [];
    public Dictionary<string, string> Facts { get; set; } = [];
}

public sealed class HistoryItem
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public sealed class ChatResponse
{
    public string Reply { get; set; } = string.Empty;
}
