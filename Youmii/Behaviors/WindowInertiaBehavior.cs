using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Youmii.Behaviors;

/// <summary>
/// Provides drag inertia behavior for a Window.
/// Works in device pixel coordinates for accurate screen bounds.
/// </summary>
public sealed class WindowInertiaBehavior : IDisposable
{
    private const double Friction = 0.92;
    private const double MinVelocity = 5.0;
    private const double CollisionThreshold = 800.0;
    private const int TickIntervalMs = 16;
    private const int VelocitySamples = 3;
    private const double SampleExpirationMs = 100.0;

    private readonly Window _window;
    private readonly DispatcherTimer _inertiaTimer;
    private readonly Queue<VelocitySample> _velocityHistory;

    private Point _lastMouseScreenPosition;
    private DateTime _lastMoveTime;
    private double _velocityX;
    private double _velocityY;
    private bool _isDragging;
    private bool _disposed;

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
            // Delta in pixels
            var deltaXPixels = screenPositionPixels.X - _lastMouseScreenPosition.X;
            var deltaYPixels = screenPositionPixels.Y - _lastMouseScreenPosition.Y;

            // Get DPI scale for this movement
            var dpiScale = GetDpiScale();
            
            // Convert delta to WPF logical units for window movement
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
    /// Gets the actual window size in pixels using Win32 API.
    /// </summary>
    private (double Width, double Height) GetWindowSizeInPixels()
    {
        var hwnd = new WindowInteropHelper(_window).Handle;
        if (hwnd != IntPtr.Zero && GetWindowRect(hwnd, out RECT rect))
        {
            return (rect.Right - rect.Left, rect.Bottom - rect.Top);
        }
        
        // Fallback
        var dpi = GetDpiScale();
        return (_window.ActualWidth * dpi, _window.ActualHeight * dpi);
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
        
        // Fallback
        var dpi = GetDpiScale();
        return (_window.Left * dpi, _window.Top * dpi);
    }

    private void OnInertiaTick(object? sender, EventArgs e)
    {
        var tickSeconds = TickIntervalMs / 1000.0;
        var dpiScale = GetDpiScale();
        
        // Velocity is in pixels per second
        var moveXPixels = _velocityX * tickSeconds;
        var moveYPixels = _velocityY * tickSeconds;

        // Get current window position and size in PIXELS (from Win32)
        var (currentLeftPixels, currentTopPixels) = GetWindowPositionInPixels();
        var (windowWidthPixels, windowHeightPixels) = GetWindowSizeInPixels();

        // Apply movement in pixels
        var newLeftPixels = currentLeftPixels + moveXPixels;
        var newTopPixels = currentTopPixels + moveYPixels;

        // Get bounds (in pixels)
        var (left, top, right, bottom) = _useCustomBounds 
            ? (_customLeft, _customTop, _customRight, _customBottom)
            : (0.0, 0.0, (double)GetSystemMetrics(0), (double)GetSystemMetrics(1));

        var collisionOccurred = false;
        var collisionSpeed = 0.0;

        // Check bounds in pixel coordinates
        if (newLeftPixels < left)
        {
            collisionSpeed = Math.Max(collisionSpeed, Math.Abs(_velocityX));
            newLeftPixels = left;
            _velocityX = -_velocityX * 0.3;
            collisionOccurred = true;
        }
        else if (newLeftPixels + windowWidthPixels > right)
        {
            collisionSpeed = Math.Max(collisionSpeed, Math.Abs(_velocityX));
            newLeftPixels = right - windowWidthPixels;
            _velocityX = -_velocityX * 0.3;
            collisionOccurred = true;
        }

        if (newTopPixels < top)
        {
            collisionSpeed = Math.Max(collisionSpeed, Math.Abs(_velocityY));
            newTopPixels = top;
            _velocityY = -_velocityY * 0.3;
            collisionOccurred = true;
        }
        else if (newTopPixels + windowHeightPixels > bottom)
        {
            collisionSpeed = Math.Max(collisionSpeed, Math.Abs(_velocityY));
            newTopPixels = bottom - windowHeightPixels;
            _velocityY = -_velocityY * 0.3;
            collisionOccurred = true;
        }

        // Convert back to WPF units and apply
        _window.Left = newLeftPixels / dpiScale;
        _window.Top = newTopPixels / dpiScale;

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
