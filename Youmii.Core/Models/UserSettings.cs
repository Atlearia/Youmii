using Youmii.Core.Interfaces;

namespace Youmii.Core.Models;

/// <summary>
/// User-configurable settings with default values.
/// </summary>
public sealed class UserSettings : IUserSettings
{
    /// <inheritdoc />
    public string CharacterName { get; set; } = "Youmii";

    /// <inheritdoc />
    public int IdleMinIntervalSeconds { get; set; } = 30;

    /// <inheritdoc />
    public int IdleMaxIntervalSeconds { get; set; } = 60;

    /// <inheritdoc />
    public bool IdleMessagesEnabled { get; set; } = true;

    /// <inheritdoc />
    public int BubbleDisplaySeconds { get; set; } = 8;

    /// <inheritdoc />
    public bool AlwaysOnTop { get; set; } = true;

    /// <inheritdoc />
    public double CharacterOpacity { get; set; } = 1.0;

    /// <inheritdoc />
    public bool SoundEffectsEnabled { get; set; } = true;

    /// <inheritdoc />
    public bool StartWithWindows { get; set; } = false;

    /// <inheritdoc />
    public double CharacterScale { get; set; } = 1.0;

    // New Personality Settings

    /// <inheritdoc />
    public string PersonalityType { get; set; } = "cheerful";

    /// <inheritdoc />
    public string SpeechStyle { get; set; } = "cute";

    /// <inheritdoc />
    public int ChattinessLevel { get; set; } = 50;

    // New Theme Settings

    /// <inheritdoc />
    public string AccentColor { get; set; } = "#FFE91E63";

    /// <inheritdoc />
    public string BubbleStyle { get; set; } = "round";

    /// <inheritdoc />
    public bool DarkModeEnabled { get; set; } = false;

    // New Effects Settings

    /// <inheritdoc />
    public bool BounceAnimationEnabled { get; set; } = true;

    /// <inheritdoc />
    public bool SparkleEffectsEnabled { get; set; } = true;

    /// <inheritdoc />
    public bool TypingAnimationEnabled { get; set; } = true;

    /// <inheritdoc />
    public double AnimationSpeed { get; set; } = 1.0;

    // AI Settings

    /// <inheritdoc />
    public string BrainClientType { get; set; } = "Auto";

    /// <inheritdoc />
    public string OllamaUrl { get; set; } = "http://localhost:11434";

    /// <inheritdoc />
    public string OllamaModel { get; set; } = "llama3.2";

    /// <summary>
    /// Creates a deep copy of the settings.
    /// </summary>
    public UserSettings Clone()
    {
        return new UserSettings
        {
            CharacterName = CharacterName,
            IdleMinIntervalSeconds = IdleMinIntervalSeconds,
            IdleMaxIntervalSeconds = IdleMaxIntervalSeconds,
            IdleMessagesEnabled = IdleMessagesEnabled,
            BubbleDisplaySeconds = BubbleDisplaySeconds,
            AlwaysOnTop = AlwaysOnTop,
            CharacterOpacity = CharacterOpacity,
            SoundEffectsEnabled = SoundEffectsEnabled,
            StartWithWindows = StartWithWindows,
            CharacterScale = CharacterScale,
            // Personality properties
            PersonalityType = PersonalityType,
            SpeechStyle = SpeechStyle,
            ChattinessLevel = ChattinessLevel,
            AccentColor = AccentColor,
            BubbleStyle = BubbleStyle,
            DarkModeEnabled = DarkModeEnabled,
            BounceAnimationEnabled = BounceAnimationEnabled,
            SparkleEffectsEnabled = SparkleEffectsEnabled,
            TypingAnimationEnabled = TypingAnimationEnabled,
            AnimationSpeed = AnimationSpeed,
            // AI properties
            BrainClientType = BrainClientType,
            OllamaUrl = OllamaUrl,
            OllamaModel = OllamaModel
        };
    }

    /// <summary>
    /// Copies values from another settings instance.
    /// </summary>
    public void CopyFrom(IUserSettings other)
    {
        CharacterName = other.CharacterName;
        IdleMinIntervalSeconds = other.IdleMinIntervalSeconds;
        IdleMaxIntervalSeconds = other.IdleMaxIntervalSeconds;
        IdleMessagesEnabled = other.IdleMessagesEnabled;
        BubbleDisplaySeconds = other.BubbleDisplaySeconds;
        AlwaysOnTop = other.AlwaysOnTop;
        CharacterOpacity = other.CharacterOpacity;
        SoundEffectsEnabled = other.SoundEffectsEnabled;
        StartWithWindows = other.StartWithWindows;
        CharacterScale = other.CharacterScale;
        // Personality properties
        PersonalityType = other.PersonalityType;
        SpeechStyle = other.SpeechStyle;
        ChattinessLevel = other.ChattinessLevel;
        AccentColor = other.AccentColor;
        BubbleStyle = other.BubbleStyle;
        DarkModeEnabled = other.DarkModeEnabled;
        BounceAnimationEnabled = other.BounceAnimationEnabled;
        SparkleEffectsEnabled = other.SparkleEffectsEnabled;
        TypingAnimationEnabled = other.TypingAnimationEnabled;
        AnimationSpeed = other.AnimationSpeed;
        // AI properties
        BrainClientType = other.BrainClientType;
        OllamaUrl = other.OllamaUrl;
        OllamaModel = other.OllamaModel;
    }

    /// <summary>
    /// Resets all settings to their default values.
    /// </summary>
    public void ResetToDefaults()
    {
        CharacterName = "Youmii";
        IdleMinIntervalSeconds = 30;
        IdleMaxIntervalSeconds = 60;
        IdleMessagesEnabled = true;
        BubbleDisplaySeconds = 8;
        AlwaysOnTop = true;
        CharacterOpacity = 1.0;
        SoundEffectsEnabled = true;
        StartWithWindows = false;
        CharacterScale = 1.0;
        // Personality properties
        PersonalityType = "cheerful";
        SpeechStyle = "cute";
        ChattinessLevel = 50;
        AccentColor = "#FFE91E63";
        BubbleStyle = "round";
        DarkModeEnabled = false;
        BounceAnimationEnabled = true;
        SparkleEffectsEnabled = true;
        TypingAnimationEnabled = true;
        AnimationSpeed = 1.0;
        // AI properties
        BrainClientType = "Auto";
        OllamaUrl = "http://localhost:11434";
        OllamaModel = "llama3.2";
    }
}
