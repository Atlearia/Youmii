using System.Windows.Threading;
using Youmii.Core.Services;

namespace Youmii.Features.IdleMessages.Services;

/// <summary>
/// Coordinates idle message display including timer management and message selection.
/// Extracts idle message logic from MainViewModel for better separation of concerns.
/// </summary>
public sealed class IdleMessageCoordinator : IDisposable
{
    private readonly IdleMessageService _idleMessageService;
    private readonly DispatcherTimer _idleTimer;
    private bool _isPaused;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="IdleMessageCoordinator"/> class.
    /// </summary>
    public IdleMessageCoordinator()
    {
        _idleMessageService = new IdleMessageService();
        _idleTimer = new DispatcherTimer();
        _idleTimer.Tick += OnIdleTimerTick;
    }

    /// <summary>
    /// Event raised when an idle message should be displayed.
    /// </summary>
    public event EventHandler<IdleMessageEventArgs>? IdleMessageTriggered;

    /// <summary>
    /// Gets whether idle messages are enabled.
    /// </summary>
    public bool IsEnabled => _idleMessageService.IsEnabled;

    /// <summary>
    /// Gets or sets whether the idle timer is temporarily paused.
    /// Use this when the user is actively interacting (e.g., typing, loading).
    /// </summary>
    public bool IsPaused
    {
        get => _isPaused;
        set
        {
            _isPaused = value;
            if (_isPaused)
            {
                _idleTimer.Stop();
            }
            else if (_idleMessageService.IsEnabled)
            {
                ResetTimer();
            }
        }
    }

    /// <summary>
    /// Configures the idle message service with settings.
    /// </summary>
    /// <param name="minSeconds">Minimum interval in seconds.</param>
    /// <param name="maxSeconds">Maximum interval in seconds.</param>
    /// <param name="enabled">Whether idle messages are enabled.</param>
    public void Configure(int minSeconds, int maxSeconds, bool enabled)
    {
        _idleMessageService.Configure(minSeconds, maxSeconds, enabled);

        if (!enabled)
        {
            _idleTimer.Stop();
        }
        else if (!_isPaused)
        {
            ResetTimer();
        }
    }

    /// <summary>
    /// Resets the idle timer. Call this on any user interaction.
    /// </summary>
    public void ResetTimer()
    {
        _idleTimer.Stop();

        if (_idleMessageService.IsEnabled && !_isPaused)
        {
            _idleTimer.Interval = _idleMessageService.GetRandomInterval();
            _idleTimer.Start();
        }
    }

    /// <summary>
    /// Starts the idle timer if enabled.
    /// </summary>
    public void Start()
    {
        if (_idleMessageService.IsEnabled && !_isPaused)
        {
            ResetTimer();
        }
    }

    /// <summary>
    /// Stops the idle timer.
    /// </summary>
    public void Stop()
    {
        _idleTimer.Stop();
    }

    private void OnIdleTimerTick(object? sender, EventArgs e)
    {
        // Don't show messages if paused or disabled
        if (_isPaused || !_idleMessageService.IsEnabled)
        {
            ResetTimer();
            return;
        }

        // Get and raise the message
        var message = _idleMessageService.GetRandomMessage();
        IdleMessageTriggered?.Invoke(this, new IdleMessageEventArgs(message));

        // Schedule next message
        ResetTimer();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _idleTimer.Stop();
    }
}

/// <summary>
/// Event arguments for idle messages.
/// </summary>
public sealed class IdleMessageEventArgs : EventArgs
{
    public IdleMessageEventArgs(string message)
    {
        Message = message;
    }

    /// <summary>
    /// Gets the idle message to display.
    /// </summary>
    public string Message { get; }
}
