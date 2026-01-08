using Youmii.Core.Interfaces;
using Youmii.Core.Services;
using Youmii.Features.Settings.ViewModels;
using Youmii.Features.Settings.Views;

namespace Youmii.Features.Settings.Services;

/// <summary>
/// Coordinates settings operations including loading, saving, and applying settings.
/// Extracts settings-related logic from MainViewModel for better separation of concerns.
/// </summary>
public sealed class SettingsCoordinator : IDisposable
{
    private readonly SettingsService _settingsService;
    private readonly StartupService _startupService;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsCoordinator"/> class.
    /// </summary>
    public SettingsCoordinator()
    {
        _settingsService = new SettingsService();
        _startupService = new StartupService();
    }

    /// <summary>
    /// Gets the settings service for external access.
    /// </summary>
    public ISettingsService SettingsService => _settingsService;

    /// <summary>
    /// Gets the current settings.
    /// </summary>
    public IUserSettings CurrentSettings => _settingsService.CurrentSettings;

    /// <summary>
    /// Event raised when settings have been applied.
    /// </summary>
    public event EventHandler<SettingsAppliedEventArgs>? SettingsApplied;

    /// <summary>
    /// Loads settings from persistent storage.
    /// </summary>
    public async Task LoadSettingsAsync()
    {
        await _settingsService.LoadAsync();
    }

    /// <summary>
    /// Applies current settings and raises the SettingsApplied event.
    /// </summary>
    public void ApplySettings()
    {
        var settings = _settingsService.CurrentSettings;

        // Sync startup with Windows
        _startupService.SyncWithSettings(settings.StartWithWindows);

        // Raise event with applied settings
        SettingsApplied?.Invoke(this, new SettingsAppliedEventArgs(settings));
    }

    /// <summary>
    /// Opens the settings window and returns whether changes were saved.
    /// </summary>
    /// <returns>True if settings were saved, false if cancelled.</returns>
    public bool OpenSettingsDialog()
    {
        var settingsVm = new SettingsViewModel(_settingsService);
        var settingsWindow = new SettingsWindow();
        settingsWindow.SetViewModel(settingsVm);

        var result = settingsWindow.ShowDialog();

        if (result == true)
        {
            ApplySettings();
            return true;
        }

        return false;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _startupService.Dispose();
    }
}

/// <summary>
/// Event arguments for when settings are applied.
/// </summary>
public sealed class SettingsAppliedEventArgs : EventArgs
{
    public SettingsAppliedEventArgs(IUserSettings settings)
    {
        Settings = settings;
    }

    /// <summary>
    /// Gets the applied settings.
    /// </summary>
    public IUserSettings Settings { get; }
}
