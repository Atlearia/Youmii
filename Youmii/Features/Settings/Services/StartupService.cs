using Microsoft.Win32;

namespace Youmii.Features.Settings.Services;

/// <summary>
/// Service for managing Windows startup registration.
/// Handles adding/removing the application from Windows startup.
/// </summary>
public sealed class StartupService : IDisposable
{
    private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "Youmii";
    private readonly string _executablePath;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartupService"/> class.
    /// </summary>
    public StartupService()
    {
        // Get the executable path
        _executablePath = Environment.ProcessPath ?? 
            System.Reflection.Assembly.GetExecutingAssembly().Location;
    }

    /// <summary>
    /// Gets whether the application is currently set to start with Windows.
    /// </summary>
    public bool IsStartupEnabled
    {
        get
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
                if (key == null) return false;
                
                var value = key.GetValue(AppName) as string;
                return !string.IsNullOrEmpty(value) && 
                       value.Equals(_executablePath, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Enables or disables the application starting with Windows.
    /// </summary>
    /// <param name="enable">True to enable startup, false to disable.</param>
    /// <returns>True if the operation succeeded.</returns>
    public bool SetStartupEnabled(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
            if (key == null) return false;

            if (enable)
            {
                key.SetValue(AppName, _executablePath);
            }
            else
            {
                key.DeleteValue(AppName, false);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Synchronizes the startup state with the provided setting value.
    /// </summary>
    /// <param name="shouldStartWithWindows">The desired startup state from settings.</param>
    public void SyncWithSettings(bool shouldStartWithWindows)
    {
        var currentState = IsStartupEnabled;
        if (currentState != shouldStartWithWindows)
        {
            SetStartupEnabled(shouldStartWithWindows);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        // No unmanaged resources to dispose
    }
}
