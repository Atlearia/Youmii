using Youmii.Core.Data;
using Youmii.Core.Interfaces;

namespace Youmii.Core.Services;

/// <summary>
/// Default implementation of the idle message service.
/// Provides random messages from a predefined collection with configurable intervals.
/// </summary>
public sealed class IdleMessageService : IIdleMessageService
{
    private readonly Random _random;
    private int _lastMessageIndex = -1;
    private int _minIntervalSeconds = 30;
    private int _maxIntervalSeconds = 60;
    private bool _isEnabled = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="IdleMessageService"/> class.
    /// </summary>
    public IdleMessageService()
    {
        _random = new Random();
    }

    /// <inheritdoc />
    public int MinIntervalSeconds
    {
        get => _minIntervalSeconds;
        set => _minIntervalSeconds = Math.Max(10, value);
    }

    /// <inheritdoc />
    public int MaxIntervalSeconds
    {
        get => _maxIntervalSeconds;
        set => _maxIntervalSeconds = Math.Max(MinIntervalSeconds, value);
    }

    /// <summary>
    /// Gets or sets whether idle messages are enabled.
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set => _isEnabled = value;
    }

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

    /// <summary>
    /// Configures the service with settings from user preferences.
    /// </summary>
    /// <param name="minSeconds">Minimum interval in seconds.</param>
    /// <param name="maxSeconds">Maximum interval in seconds.</param>
    /// <param name="enabled">Whether idle messages are enabled.</param>
    public void Configure(int minSeconds, int maxSeconds, bool enabled)
    {
        MinIntervalSeconds = minSeconds;
        MaxIntervalSeconds = maxSeconds;
        IsEnabled = enabled;
    }
}
