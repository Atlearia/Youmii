namespace Youmii.Core.Models;

/// <summary>
/// Represents a message in the conversation history.
/// </summary>
public sealed class ChatMessage
{
    public long Id { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static ChatMessage User(string content) => new()
    {
        Role = MessageRoles.User,
        Content = content,
        CreatedAt = DateTime.UtcNow
    };

    public static ChatMessage Assistant(string content) => new()
    {
        Role = MessageRoles.Assistant,
        Content = content,
        CreatedAt = DateTime.UtcNow
    };
}

/// <summary>
/// Constants for message roles.
/// </summary>
public static class MessageRoles
{
    public const string User = "user";
    public const string Assistant = "assistant";
}
