namespace Youmii.Core.Models;

/// <summary>
/// Request payload for the brain API.
/// </summary>
public sealed class BrainRequest
{
    public string Message { get; set; } = string.Empty;
    public List<BrainHistoryItem> History { get; set; } = [];
    public Dictionary<string, string> Facts { get; set; } = [];
}

/// <summary>
/// A history item for the brain API.
/// </summary>
public sealed class BrainHistoryItem
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public static BrainHistoryItem FromChatMessage(ChatMessage msg) => new()
    {
        Role = msg.Role,
        Content = msg.Content
    };
}

/// <summary>
/// Response payload from the brain API.
/// </summary>
public sealed class BrainResponse
{
    public string Reply { get; set; } = string.Empty;
}
