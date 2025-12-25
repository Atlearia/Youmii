namespace Youmii.Core.Mvvm;

/// <summary>
/// Base class for ViewModels with disposal support.
/// </summary>
public abstract class ViewModelBase : ObservableObject, IDisposable
{
    private bool _disposed;

    /// <summary>
    /// Gets whether this ViewModel has been disposed.
    /// </summary>
    public bool IsDisposed => _disposed;

    /// <summary>
    /// Releases all resources used by this ViewModel.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        Dispose(disposing: true);
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Override to release managed resources.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        // Override in derived classes to release resources
    }

    /// <summary>
    /// Throws if this ViewModel has been disposed.
    /// </summary>
    protected void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
