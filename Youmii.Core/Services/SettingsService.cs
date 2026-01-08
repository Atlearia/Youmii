using System.Text.Json;
using Youmii.Core.Interfaces;
using Youmii.Core.Models;

namespace Youmii.Core.Services;

/// <summary>
/// Service for persisting user settings to JSON file.
/// </summary>
public sealed class SettingsService : ISettingsService
{
    private readonly string _settingsPath;
    private readonly UserSettings _settings;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsService"/> class.
    /// </summary>
    /// <param name="settingsPath">Optional path to settings file. Defaults to user's AppData folder.</param>
    public SettingsService(string? settingsPath = null)
    {
        if (string.IsNullOrEmpty(settingsPath))
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var youmiiPath = Path.Combine(appDataPath, "Youmii");
            Directory.CreateDirectory(youmiiPath);
            _settingsPath = Path.Combine(youmiiPath, "usersettings.json");
        }
        else
        {
            _settingsPath = settingsPath;
        }

        _settings = new UserSettings();
    }

    /// <inheritdoc />
    public IUserSettings CurrentSettings => _settings;

    /// <inheritdoc />
    public event EventHandler? SettingsChanged;

    /// <inheritdoc />
    public async Task LoadAsync()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = await File.ReadAllTextAsync(_settingsPath);
                var loaded = JsonSerializer.Deserialize<UserSettings>(json, JsonOptions);
                if (loaded != null)
                {
                    _settings.CopyFrom(loaded);
                }
            }
        }
        catch
        {
            // If loading fails, use defaults
            _settings.ResetToDefaults();
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_settings, JsonOptions);
            await File.WriteAllTextAsync(_settingsPath, json);
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
        catch
        {
            // Silently fail on save errors - settings will be in-memory only
        }
    }

    /// <inheritdoc />
    public void ResetToDefaults()
    {
        _settings.ResetToDefaults();
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the SettingsChanged event.
    /// Call this after modifying settings to notify listeners.
    /// </summary>
    public void NotifySettingsChanged()
    {
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }
}
