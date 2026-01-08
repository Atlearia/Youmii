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
            CharacterScale = CharacterScale
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
    }
}
