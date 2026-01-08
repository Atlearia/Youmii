namespace Youmii.Core.Interfaces;

/// <summary>
/// Service for managing user-configurable settings persistence.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Gets the current user settings.
    /// </summary>
    IUserSettings CurrentSettings { get; }

    /// <summary>
    /// Saves the current settings to persistent storage.
    /// </summary>
    Task SaveAsync();

    /// <summary>
    /// Loads settings from persistent storage.
    /// </summary>
    Task LoadAsync();

    /// <summary>
    /// Resets all settings to default values.
    /// </summary>
    void ResetToDefaults();

    /// <summary>
    /// Event raised when settings are changed.
    /// </summary>
    event EventHandler? SettingsChanged;
}

/// <summary>
/// User-configurable settings interface.
/// </summary>
public interface IUserSettings
{
    /// <summary>
    /// Gets or sets the character's display name.
    /// </summary>
    string CharacterName { get; set; }

    /// <summary>
    /// Gets or sets the minimum idle interval in seconds before the character speaks.
    /// </summary>
    int IdleMinIntervalSeconds { get; set; }

    /// <summary>
    /// Gets or sets the maximum idle interval in seconds before the character speaks.
    /// </summary>
    int IdleMaxIntervalSeconds { get; set; }

    /// <summary>
    /// Gets or sets whether idle messages are enabled.
    /// </summary>
    bool IdleMessagesEnabled { get; set; }

    /// <summary>
    /// Gets or sets how long speech bubbles remain visible in seconds.
    /// </summary>
    int BubbleDisplaySeconds { get; set; }

    /// <summary>
    /// Gets or sets whether the character should always be on top.
    /// </summary>
    bool AlwaysOnTop { get; set; }

    /// <summary>
    /// Gets or sets the character opacity (0.0 to 1.0).
    /// </summary>
    double CharacterOpacity { get; set; }

    /// <summary>
    /// Gets or sets whether sound effects are enabled.
    /// </summary>
    bool SoundEffectsEnabled { get; set; }

    /// <summary>
    /// Gets or sets whether startup with Windows is enabled.
    /// </summary>
    bool StartWithWindows { get; set; }

    /// <summary>
    /// Gets or sets the character scale (0.5 to 2.0).
    /// </summary>
    double CharacterScale { get; set; }
}
