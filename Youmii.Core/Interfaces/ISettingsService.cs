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
    // Basic Settings
    string CharacterName { get; set; }
    int IdleMinIntervalSeconds { get; set; }
    int IdleMaxIntervalSeconds { get; set; }
    bool IdleMessagesEnabled { get; set; }
    int BubbleDisplaySeconds { get; set; }
    bool AlwaysOnTop { get; set; }
    double CharacterOpacity { get; set; }
    bool SoundEffectsEnabled { get; set; }
    bool StartWithWindows { get; set; }
    double CharacterScale { get; set; }

    // Personality Settings
    string PersonalityType { get; set; }
    string SpeechStyle { get; set; }
    int ChattinessLevel { get; set; }

    // Theme Settings
    string AccentColor { get; set; }
    string BubbleStyle { get; set; }
    bool DarkModeEnabled { get; set; }

    // Effects Settings
    bool BounceAnimationEnabled { get; set; }
    bool SparkleEffectsEnabled { get; set; }
    bool TypingAnimationEnabled { get; set; }
    double AnimationSpeed { get; set; }

    // AI Settings
    string BrainClientType { get; set; }
    string OllamaUrl { get; set; }
    string OllamaModel { get; set; }

    // Screen Bounds Settings
    bool CustomScreenBoundsEnabled { get; set; }
    double ScreenBoundsLeft { get; set; }
    double ScreenBoundsTop { get; set; }
    double ScreenBoundsRight { get; set; }
    double ScreenBoundsBottom { get; set; }
}
