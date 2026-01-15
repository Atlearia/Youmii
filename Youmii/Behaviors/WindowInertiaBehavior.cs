using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Youmii.Behaviors;

/// <summary>
/// Provides drag inertia behavior for a Window.
/// Works in device pixel coordinates for accurate screen bounds.
/// Collision detection uses character bounds, not window bounds.
/// </summary>
public sealed class WindowInertiaBehavior : IDisposable
{
    private const double Friction = 0.92;
    private const double MinVelocity = 5.0;
    private const double CollisionThreshold = 800.0;
    private const int TickIntervalMs = 16;
    private const int VelocitySamples = 3;
    private const double SampleExpirationMs = 100.0;

    // Character base size in WPF logical units (from XAML: Width="200" Height="200")
    private const double CharacterBaseSize = 200.0;
    
    // Character offset from window top (speech bubble + margins)
    // From XAML: Grid.Row="1" with Margin="0,15,0,10", plus speech bubble above
    private const double CharacterTopOffsetWpf = 100.0; // Approximate offset from window top to character center area

    private readonly Window _window;
    private readonly DispatcherTimer _inertiaTimer;
    private readonly Queue<VelocitySample> _velocityHistory;

    private Point _lastMouseScreenPosition;
    private DateTime _lastMoveTime;
    private double _velocityX;
    private double _velocityY;
    private bool _isDragging;
    private bool _disposed;

    // Character scale (1.0 = 100%, 2.0 = 200%)
    private double _characterScale = 1.0;

    // Custom screen bounds in PIXELS
    private bool _useCustomBounds;
    private double _customLeft;
    private double _customTop;
    private double _customRight;
    private double _customBottom;

    public event EventHandler? BoundaryCollision;

    public WindowInertiaBehavior(Window window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
        _velocityHistory = new Queue<VelocitySample>(VelocitySamples + 1);

        _inertiaTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(TickIntervalMs)
        };
        _inertiaTimer.Tick += OnInertiaTick;
    }

    /// <summary>
    /// Sets the character scale for accurate collision detection.
    /// </summary>
    public void SetCharacterScale(double scale)
    {
        _characterScale = Math.Max(0.5, Math.Min(2.0, scale));
    }

    /// <summary>
    /// Sets custom screen bounds in PIXEL coordinates.
    /// </summary>
    public void SetCustomBounds(bool enabled, double left, double top, double right, double bottom)
    {
        _useCustomBounds = enabled;
        _customLeft = left;
        _customTop = top;
        _customRight = right;
        _customBottom = bottom;
    }

    public void StartDrag(Point screenPositionPixels)
    {
        _inertiaTimer.Stop();
        _velocityX = 0;
        _velocityY = 0;
        _velocityHistory.Clear();
        _lastMouseScreenPosition = screenPositionPixels;
        _lastMoveTime = DateTime.UtcNow;
        _isDragging = true;
    }

    public void UpdateDrag(Point screenPositionPixels)
    {
        if (!_isDragging) return;

        var now = DateTime.UtcNow;
        var deltaTime = (now - _lastMoveTime).TotalSeconds;

        if (deltaTime > 0.001)
        {
            var deltaXPixels = screenPositionPixels.X - _lastMouseScreenPosition.X;
            var deltaYPixels = screenPositionPixels.Y - _lastMouseScreenPosition.Y;

            var dpiScale = GetDpiScale();
            
            var deltaXWpf = deltaXPixels / dpiScale;
            var deltaYWpf = deltaYPixels / dpiScale;

            _window.Left += deltaXWpf;
            _window.Top += deltaYWpf;

            if (Math.Abs(deltaXPixels) > 0.5 || Math.Abs(deltaYPixels) > 0.5)
            {
                var instantVelX = deltaXPixels / deltaTime;
                var instantVelY = deltaYPixels / deltaTime;

                _velocityHistory.Enqueue(new VelocitySample(instantVelX, instantVelY, now));

                while (_velocityHistory.Count > VelocitySamples)
                    _velocityHistory.Dequeue();
            }

            while (_velocityHistory.Count > 0 && 
                   (now - _velocityHistory.Peek().Timestamp).TotalMilliseconds > SampleExpirationMs)
                _velocityHistory.Dequeue();
        }

        _lastMouseScreenPosition = screenPositionPixels;
        _lastMoveTime = now;
    }

    public void EndDrag()
    {
        if (!_isDragging) return;
        _isDragging = false;

        var now = DateTime.UtcNow;

        while (_velocityHistory.Count > 0 && 
               (now - _velocityHistory.Peek().Timestamp).TotalMilliseconds > SampleExpirationMs)
            _velocityHistory.Dequeue();

        if (_velocityHistory.Count > 0)
        {
            var avgVelX = 0.0;
            var avgVelY = 0.0;

            foreach (var sample in _velocityHistory)
            {
                var age = (now - sample.Timestamp).TotalMilliseconds;
                var weight = Math.Max(0.1, 1.0 - (age / SampleExpirationMs));
                avgVelX += sample.VelocityX * weight;
                avgVelY += sample.VelocityY * weight;
            }

            _velocityX = avgVelX / _velocityHistory.Count;
            _velocityY = avgVelY / _velocityHistory.Count;

            var maxVel = 2500.0;
            _velocityX = Math.Clamp(_velocityX, -maxVel, maxVel);
            _velocityY = Math.Clamp(_velocityY, -maxVel, maxVel);
        }
        else
        {
            _velocityX = 0;
            _velocityY = 0;
        }

        if (Math.Sqrt(_velocityX * _velocityX + _velocityY * _velocityY) > MinVelocity)
            _inertiaTimer.Start();
    }

    public void StopInertia()
    {
        _inertiaTimer.Stop();
        _velocityX = 0;
        _velocityY = 0;
    }

    private double GetDpiScale()
    {
        int screenPixels = GetSystemMetrics(0); // SM_CXSCREEN
        return screenPixels / SystemParameters.PrimaryScreenWidth;
    }

    /// <summary>
    /// Gets the actual window position in pixels using Win32 API.
    /// </summary>
    private (double Left, double Top) GetWindowPositionInPixels()
    {
        var hwnd = new WindowInteropHelper(_window).Handle;
        if (hwnd != IntPtr.Zero && GetWindowRect(hwnd, out RECT rect))
        {
            return (rect.Left, rect.Top);
        }
        
        var dpi = GetDpiScale();
        return (_window.Left * dpi, _window.Top * dpi);
    }

    /// <summary>
    /// Gets the character bounds in screen pixel coordinates.
    /// The character is centered horizontally in the window with an offset from top.
    /// </summary>
    private (double Left, double Top, double Right, double Bottom) GetCharacterBoundsInPixels()
    {
        var dpiScale = GetDpiScale();
        var (windowLeftPixels, windowTopPixels) = GetWindowPositionInPixels();

        // Character size in pixels (base size * scale * DPI)
        var characterSizePixels = CharacterBaseSize * _characterScale * dpiScale;

        // Window width in pixels
        var hwnd = new WindowInteropHelper(_window).Handle;
        double windowWidthPixels = _window.ActualWidth * dpiScale;
        if (hwnd != IntPtr.Zero && GetWindowRect(hwnd, out RECT rect))
        {
            windowWidthPixels = rect.Right - rect.Left;
        }

        // Character is horizontally centered in the window
        var characterLeftOffset = (windowWidthPixels - characterSizePixels) / 2.0;
        
        // Character vertical offset from window top (in pixels)
        var characterTopOffset = CharacterTopOffsetWpf * dpiScale;

        var charLeft = windowLeftPixels + characterLeftOffset;
        var charTop = windowTopPixels + characterTopOffset;
        var charRight = charLeft + characterSizePixels;
        var charBottom = charTop + characterSizePixels;

        return (charLeft, charTop, charRight, charBottom);
    }

    private void OnInertiaTick(object? sender, EventArgs e)
    {
        var tickSeconds = TickIntervalMs / 1000.0;
        var dpiScale = GetDpiScale();
        
        // Velocity is in pixels per second
        var moveXPixels = _velocityX * tickSeconds;
        var moveYPixels = _velocityY * tickSeconds;

        // Get current CHARACTER bounds (not window bounds)
        var (charLeft, charTop, charRight, charBottom) = GetCharacterBoundsInPixels();

        // Calculate new character position after movement
        var newCharLeft = charLeft + moveXPixels;
        var newCharTop = charTop + moveYPixels;
        var newCharRight = charRight + moveXPixels;
        var newCharBottom = charBottom + moveYPixels;

        // Get screen bounds (in pixels)
        var (boundLeft, boundTop, boundRight, boundBottom) = _useCustomBounds 
            ? (_customLeft, _customTop, _customRight, _customBottom)
            : (0.0, 0.0, (double)GetSystemMetrics(0), (double)GetSystemMetrics(1));

        var collisionOccurred = false;
        var collisionSpeed = 0.0;

        // Adjustment to apply to window position
        var adjustX = 0.0;
        var adjustY = 0.0;

        // Check LEFT boundary (character left edge)
        if (newCharLeft < boundLeft)
        {
            collisionSpeed = Math.Max(collisionSpeed, Math.Abs(_velocityX));
            adjustX = boundLeft - newCharLeft;
            _velocityX = -_velocityX * 0.3;
            collisionOccurred = true;
        }
        // Check RIGHT boundary (character right edge)
        else if (newCharRight > boundRight)
        {
            collisionSpeed = Math.Max(collisionSpeed, Math.Abs(_velocityX));
            adjustX = boundRight - newCharRight;
            _velocityX = -_velocityX * 0.3;
            collisionOccurred = true;
        }

        // Check TOP boundary (character top edge)
        if (newCharTop < boundTop)
        {
            collisionSpeed = Math.Max(collisionSpeed, Math.Abs(_velocityY));
            adjustY = boundTop - newCharTop;
            _velocityY = -_velocityY * 0.3;
            collisionOccurred = true;
        }
        // Check BOTTOM boundary (character bottom edge)
        else if (newCharBottom > boundBottom)
        {
            collisionSpeed = Math.Max(collisionSpeed, Math.Abs(_velocityY));
            adjustY = boundBottom - newCharBottom;
            _velocityY = -_velocityY * 0.3;
            collisionOccurred = true;
        }

        // Apply movement to window (in WPF units)
        var totalMoveXPixels = moveXPixels + adjustX;
        var totalMoveYPixels = moveYPixels + adjustY;
        
        _window.Left += totalMoveXPixels / dpiScale;
        _window.Top += totalMoveYPixels / dpiScale;

        if (collisionOccurred && collisionSpeed > CollisionThreshold)
            BoundaryCollision?.Invoke(this, EventArgs.Empty);

        _velocityX *= Friction;
        _velocityY *= Friction;

        if (Math.Sqrt(_velocityX * _velocityX + _velocityY * _velocityY) < MinVelocity)
            _inertiaTimer.Stop();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _inertiaTimer.Stop();
        _inertiaTimer.Tick -= OnInertiaTick;
    }

    private readonly record struct VelocitySample(double VelocityX, double VelocityY, DateTime Timestamp);

    #region Win32 Interop

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    #endregion
}
