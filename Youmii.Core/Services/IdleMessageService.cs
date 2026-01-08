using Youmii.Core.Data;
using Youmii.Core.Interfaces;

namespace Youmii.Core.Services;

/// <summary>
/// Default implementation of the idle message service.
/// Provides random messages from a predefined collection.
/// </summary>
public sealed class IdleMessageService : IIdleMessageService
{
    private readonly Random _random;
    private int _lastMessageIndex = -1;

    /// <summary>
    /// Initializes a new instance of the <see cref="IdleMessageService"/> class.
    /// </summary>
    public IdleMessageService()
    {
        _random = new Random();
    }

    /// <inheritdoc />
    public int MinIntervalSeconds => 30;

    /// <inheritdoc />
    public int MaxIntervalSeconds => 60;

    /// <inheritdoc />
    public string GetRandomMessage()
    {
        var messages = IdleMessageTexts.Messages;
        
        // Avoid repeating the same message twice in a row
        int index;
        do
        {
            index = _random.Next(messages.Count);
        } while (index == _lastMessageIndex && messages.Count > 1);

        _lastMessageIndex = index;
        return messages[index];
    }

    /// <inheritdoc />
    public TimeSpan GetRandomInterval()
    {
        var seconds = _random.Next(MinIntervalSeconds, MaxIntervalSeconds + 1);
        return TimeSpan.FromSeconds(seconds);
    }
}
