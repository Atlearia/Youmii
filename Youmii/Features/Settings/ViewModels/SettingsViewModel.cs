using System.Collections.ObjectModel;
using System.Windows.Input;
using Youmii.Core.Interfaces;
using Youmii.Core.Models;
using Youmii.ViewModels;

namespace Youmii.Features.Settings.ViewModels;

/// <summary>
/// ViewModel for the Settings window.
/// Provides bindable properties and commands for settings management.
/// </summary>
public sealed class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;
    private readonly UserSettings _editingSettings;
    private bool _hasChanges;

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        
        // Create a working copy for editing
        _editingSettings = new UserSettings();
        _editingSettings.CopyFrom(_settingsService.CurrentSettings);

        // Initialize option collections
        PersonalityTypes = new ObservableCollection<PersonalityOption>
        {
            new("cheerful", "Cheerful", "Always happy and positive!"),
            new("shy", "Shy", "Quiet and a bit nervous..."),
            new("energetic", "Energetic", "Full of energy and excitement!"),
            new("calm", "Calm", "Peaceful and relaxed~"),
            new("playful", "Playful", "Loves jokes and games!"),
            new("caring", "Caring", "Sweet and nurturing~")
        };

        SpeechStyles = new ObservableCollection<SpeechStyleOption>
        {
            new("cute", "Cute", "Kawaii speech patterns~"),
            new("casual", "Casual", "Friendly and relaxed"),
            new("formal", "Formal", "Polite and proper"),
            new("playful", "Playful", "Fun and silly!"),
            new("poetic", "Poetic", "Dreamy and expressive")
        };

        BubbleStyles = new ObservableCollection<BubbleStyleOption>
        {
            new("round", "Round", "Soft and friendly"),
            new("cloud", "Cloud", "Fluffy like a cloud~"),
            new("pixel", "Pixel", "Retro game style!"),
            new("minimal", "Minimal", "Clean and simple")
        };

        AccentColors = new ObservableCollection<AccentColorOption>
        {
            new("#FFE91E63", "Pink", "Pink"),
            new("#FF9C27B0", "Purple", "Purple"),
            new("#FF2196F3", "Blue", "Blue"),
            new("#FF00BCD4", "Cyan", "Cyan"),
            new("#FF4CAF50", "Green", "Green"),
            new("#FFFFC107", "Yellow", "Yellow"),
            new("#FFFF5722", "Orange", "Orange"),
            new("#FFF44336", "Red", "Red")
        };

        // AI Options
        BrainClientTypes = new ObservableCollection<BrainClientOption>
        {
            new(Core.Models.BrainClientTypes.Auto, "Auto-detect", "Automatically uses Ollama if available, falls back to offline mode"),
            new(Core.Models.BrainClientTypes.Ollama, "Ollama (Local AI)", "Use local Ollama server for AI responses"),
            new(Core.Models.BrainClientTypes.Stub, "Offline Mode", "Uses pre-written responses (no AI)")
        };

        OllamaModels = new ObservableCollection<OllamaModelOption>
        {
            new("llama3.2", "Llama 3.2", "Meta's latest, great balance of speed and quality"),
            new("llama3.1", "Llama 3.1", "Larger and smarter, but slower"),
            new("mistral", "Mistral", "Fast and efficient"),
            new("phi3", "Phi-3", "Microsoft's compact model, very fast"),
            new("gemma2", "Gemma 2", "Google's open model"),
            new("qwen2.5", "Qwen 2.5", "Alibaba's multilingual model")
        };

        SaveCommand = new RelayCommand(Save, () => HasChanges);
        CancelCommand = new RelayCommand(Cancel);
        ResetToDefaultsCommand = new RelayCommand(ResetToDefaults);
    }

    #region Option Collections

    public ObservableCollection<PersonalityOption> PersonalityTypes { get; }
    public ObservableCollection<SpeechStyleOption> SpeechStyles { get; }
    public ObservableCollection<BubbleStyleOption> BubbleStyles { get; }
    public ObservableCollection<AccentColorOption> AccentColors { get; }
    public ObservableCollection<BrainClientOption> BrainClientTypes { get; }
    public ObservableCollection<OllamaModelOption> OllamaModels { get; }

    #endregion

    #region Existing Bindable Properties

    public string CharacterName
    {
        get => _editingSettings.CharacterName;
        set
        {
            if (_editingSettings.CharacterName != value)
            {
                _editingSettings.CharacterName = value;
                OnPropertyChanged();
                MarkAsChanged();
            }
        }
    }

    public int IdleMinIntervalSeconds
    {
        get => _editingSettings.IdleMinIntervalSeconds;
        set
        {
            value = Math.Clamp(value, 10, 300);
            if (_editingSettings.IdleMinIntervalSeconds != value)
            {
                _editingSettings.IdleMinIntervalSeconds = value;
                OnPropertyChanged();
                
                // Ensure max is always >= min
                if (_editingSettings.IdleMaxIntervalSeconds < value)
                {
                    IdleMaxIntervalSeconds = value;
                }
                MarkAsChanged();
            }
        }
    }

    public int IdleMaxIntervalSeconds
    {
        get => _editingSettings.IdleMaxIntervalSeconds;
        set
        {
            value = Math.Clamp(value, 10, 600);
            if (_editingSettings.IdleMaxIntervalSeconds != value)
            {
                _editingSettings.IdleMaxIntervalSeconds = value;
                OnPropertyChanged();
                
                // Ensure min is always <= max
                if (_editingSettings.IdleMinIntervalSeconds > value)
                {
                    IdleMinIntervalSeconds = value;
                }
                MarkAsChanged();
            }
        }
    }

    public bool IdleMessagesEnabled
    {
        get => _editingSettings.IdleMessagesEnabled;
        set
        {
            if (_editingSettings.IdleMessagesEnabled != value)
            {
                _editingSettings.IdleMessagesEnabled = value;
                OnPropertyChanged();
                MarkAsChanged();
            }
        }
    }

    public int BubbleDisplaySeconds
    {
        get => _editingSettings.BubbleDisplaySeconds;
        set
        {
            value = Math.Clamp(value, 3, 30);
            if (_editingSettings.BubbleDisplaySeconds != value)
            {
                _editingSettings.BubbleDisplaySeconds = value;
                OnPropertyChanged();
                MarkAsChanged();
            }
        }
    }

    public bool AlwaysOnTop
    {
        get => _editingSettings.AlwaysOnTop;
        set
        {
            if (_editingSettings.AlwaysOnTop != value)
            {
                _editingSettings.AlwaysOnTop = value;
                OnPropertyChanged();
                MarkAsChanged();
            }
        }
    }

    public double CharacterOpacity
    {
        get => _editingSettings.CharacterOpacity;
        set
        {
            value = Math.Clamp(value, 0.3, 1.0);
            if (Math.Abs(_editingSettings.CharacterOpacity - value) > 0.001)
            {
                _editingSettings.CharacterOpacity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CharacterOpacityPercent));
                MarkAsChanged();
            }
        }
    }

    public int CharacterOpacityPercent => (int)Math.Round(CharacterOpacity * 100);

    public bool SoundEffectsEnabled
    {
        get => _editingSettings.SoundEffectsEnabled;
        set
        {
            if (_editingSettings.SoundEffectsEnabled != value)
            {
                _editingSettings.SoundEffectsEnabled = value;
                OnPropertyChanged();
                MarkAsChanged();
            }
        }
    }

    public bool StartWithWindows
    {
        get => _editingSettings.StartWithWindows;
        set
        {
            if (_editingSettings.StartWithWindows != value)
            {
                _editingSettings.StartWithWindows = value;
                OnPropertyChanged();
                MarkAsChanged();
            }
        }
    }

    public double CharacterScale
    {
        get => _editingSettings.CharacterScale;
        set
        {
            value = Math.Clamp(value, 0.5, 2.0);
            if (Math.Abs(_editingSettings.CharacterScale - value) > 0.001)
            {
                _editingSettings.CharacterScale = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CharacterScalePercent));
                MarkAsChanged();
            }
        }
    }

    public int CharacterScalePercent => (int)Math.Round(CharacterScale * 100);

    #endregion

    #region New Personality Properties

    public string PersonalityType
    {
        get => _editingSettings.PersonalityType;
        set
        {
            if (_editingSettings.PersonalityType != value)
            {
                _editingSettings.PersonalityType = value;
                OnPropertyChanged();
                MarkAsChanged();
            }
        }
    }

    public string SpeechStyle
    {
        get => _editingSettings.SpeechStyle;
        set
        {
            if (_editingSettings.SpeechStyle != value)
            {
                _editingSettings.SpeechStyle = value;
                OnPropertyChanged();
                MarkAsChanged();
            }
        }
    }

    public int ChattinessLevel
    {
        get => _editingSettings.ChattinessLevel;
        set
        {
            value = Math.Clamp(value, 0, 100);
            if (_editingSettings.ChattinessLevel != value)
            {
                _editingSettings.ChattinessLevel = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ChattinessDescription));
                MarkAsChanged();
            }
        }
    }

    public string ChattinessDescription => ChattinessLevel switch
    {
        < 20 => "Very quiet",
        < 40 => "Shy",
        < 60 => "Balanced",
        < 80 => "Talkative",
        _ => "Very chatty!"
    };

    #endregion

    #region New Theme Properties

    public string AccentColor
    {
        get => _editingSettings.AccentColor;
        set
        {
            if (_editingSettings.AccentColor != value)
            {
                _editingSettings.AccentColor = value;
                OnPropertyChanged();
                MarkAsChanged();
            }
        }
    }

    public string BubbleStyle
    {
        get => _editingSettings.BubbleStyle;
        set
        {
            if (_editingSettings.BubbleStyle != value)
            {
                _editingSettings.BubbleStyle = value;
                OnPropertyChanged();
                MarkAsChanged();
            }
        }
    }

    public bool DarkModeEnabled
    {
        get => _editingSettings.DarkModeEnabled;
        set
        {
            if (_editingSettings.DarkModeEnabled != value)
            {
                _editingSettings.DarkModeEnabled = value;
                OnPropertyChanged();
                MarkAsChanged();
            }
        }
    }

    #endregion

    #region New Effects Properties

    public bool BounceAnimationEnabled
    {
        get => _editingSettings.BounceAnimationEnabled;
        set
        {
            if (_editingSettings.BounceAnimationEnabled != value)
            {
                _editingSettings.BounceAnimationEnabled = value;
                OnPropertyChanged();
                MarkAsChanged();
            }
        }
    }

    public bool SparkleEffectsEnabled
    {
        get => _editingSettings.SparkleEffectsEnabled;
        set
        {
            if (_editingSettings.SparkleEffectsEnabled != value)
            {
                _editingSettings.SparkleEffectsEnabled = value;
                OnPropertyChanged();
                MarkAsChanged();
            }
        }
    }

    public bool TypingAnimationEnabled
    {
        get => _editingSettings.TypingAnimationEnabled;
        set
        {
            if (_editingSettings.TypingAnimationEnabled != value)
            {
                _editingSettings.TypingAnimationEnabled = value;
                OnPropertyChanged();
                MarkAsChanged();
            }
        }
    }

    public double AnimationSpeed
    {
        get => _editingSettings.AnimationSpeed;
        set
        {
            value = Math.Clamp(value, 0.5, 2.0);
            if (Math.Abs(_editingSettings.AnimationSpeed - value) > 0.001)
            {
                _editingSettings.AnimationSpeed = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AnimationSpeedPercent));
                OnPropertyChanged(nameof(AnimationSpeedDescription));
                MarkAsChanged();
            }
        }
    }

    public int AnimationSpeedPercent => (int)Math.Round(AnimationSpeed * 100);

    public string AnimationSpeedDescription => AnimationSpeed switch
    {
        < 0.7 => "Slow & relaxed",
        < 1.0 => "Gentle",
        < 1.3 => "Normal",
        < 1.7 => "Snappy",
        _ => "Super fast!"
    };

    #endregion

    #region AI Settings Properties

    public string BrainClientType
    {
        get => _editingSettings.BrainClientType;
        set
        {
            if (_editingSettings.BrainClientType != value)
            {
                _editingSettings.BrainClientType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsOllamaSettingsVisible));
                MarkAsChanged();
            }
        }
    }

    public bool IsOllamaSettingsVisible => 
        BrainClientType == Core.Models.BrainClientTypes.Auto || 
        BrainClientType == Core.Models.BrainClientTypes.Ollama;

    public string OllamaUrl
    {
        get => _editingSettings.OllamaUrl;
        set
        {
            if (_editingSettings.OllamaUrl != value)
            {
                _editingSettings.OllamaUrl = value;
                OnPropertyChanged();
                MarkAsChanged();
            }
        }
    }

    public string OllamaModel
    {
        get => _editingSettings.OllamaModel;
        set
        {
            if (_editingSettings.OllamaModel != value)
            {
                _editingSettings.OllamaModel = value;
                OnPropertyChanged();
                MarkAsChanged();
            }
        }
    }

    #endregion

    #region HasChanges Property

    public bool HasChanges
    {
        get => _hasChanges;
        private set
        {
            if (SetProperty(ref _hasChanges, value))
            {
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
    }

    #endregion

    #region Commands

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand ResetToDefaultsCommand { get; }

    #endregion

    #region Events

    /// <summary>
    /// Event raised when the window should be closed.
    /// </summary>
    public event EventHandler<bool>? RequestClose;

    #endregion

    #region Private Methods

    private void MarkAsChanged()
    {
        HasChanges = true;
    }

    private void Save()
    {
        // Apply all changes to the service
        var settings = _settingsService.CurrentSettings;
        
        // Existing settings
        settings.CharacterName = _editingSettings.CharacterName;
        settings.IdleMinIntervalSeconds = _editingSettings.IdleMinIntervalSeconds;
        settings.IdleMaxIntervalSeconds = _editingSettings.IdleMaxIntervalSeconds;
        settings.IdleMessagesEnabled = _editingSettings.IdleMessagesEnabled;
        settings.BubbleDisplaySeconds = _editingSettings.BubbleDisplaySeconds;
        settings.AlwaysOnTop = _editingSettings.AlwaysOnTop;
        settings.CharacterOpacity = _editingSettings.CharacterOpacity;
        settings.SoundEffectsEnabled = _editingSettings.SoundEffectsEnabled;
        settings.StartWithWindows = _editingSettings.StartWithWindows;
        settings.CharacterScale = _editingSettings.CharacterScale;
        
        // Personality settings
        settings.PersonalityType = _editingSettings.PersonalityType;
        settings.SpeechStyle = _editingSettings.SpeechStyle;
        settings.ChattinessLevel = _editingSettings.ChattinessLevel;
        
        // Theme settings
        settings.AccentColor = _editingSettings.AccentColor;
        settings.BubbleStyle = _editingSettings.BubbleStyle;
        settings.DarkModeEnabled = _editingSettings.DarkModeEnabled;
        
        // Effects settings
        settings.BounceAnimationEnabled = _editingSettings.BounceAnimationEnabled;
        settings.SparkleEffectsEnabled = _editingSettings.SparkleEffectsEnabled;
        settings.TypingAnimationEnabled = _editingSettings.TypingAnimationEnabled;
        settings.AnimationSpeed = _editingSettings.AnimationSpeed;

        // AI settings
        settings.BrainClientType = _editingSettings.BrainClientType;
        settings.OllamaUrl = _editingSettings.OllamaUrl;
        settings.OllamaModel = _editingSettings.OllamaModel;

        _ = _settingsService.SaveAsync();

        HasChanges = false;
        RequestClose?.Invoke(this, true);
    }

    private void Cancel()
    {
        // Discard changes by closing without saving
        RequestClose?.Invoke(this, false);
    }

    private void ResetToDefaults()
    {
        var defaults = new UserSettings();
        
        // Existing settings
        CharacterName = defaults.CharacterName;
        IdleMinIntervalSeconds = defaults.IdleMinIntervalSeconds;
        IdleMaxIntervalSeconds = defaults.IdleMaxIntervalSeconds;
        IdleMessagesEnabled = defaults.IdleMessagesEnabled;
        BubbleDisplaySeconds = defaults.BubbleDisplaySeconds;
        AlwaysOnTop = defaults.AlwaysOnTop;
        CharacterOpacity = defaults.CharacterOpacity;
        SoundEffectsEnabled = defaults.SoundEffectsEnabled;
        StartWithWindows = defaults.StartWithWindows;
        CharacterScale = defaults.CharacterScale;
        
        // Personality settings
        PersonalityType = defaults.PersonalityType;
        SpeechStyle = defaults.SpeechStyle;
        ChattinessLevel = defaults.ChattinessLevel;
        AccentColor = defaults.AccentColor;
        BubbleStyle = defaults.BubbleStyle;
        DarkModeEnabled = defaults.DarkModeEnabled;
        BounceAnimationEnabled = defaults.BounceAnimationEnabled;
        SparkleEffectsEnabled = defaults.SparkleEffectsEnabled;
        TypingAnimationEnabled = defaults.TypingAnimationEnabled;
        AnimationSpeed = defaults.AnimationSpeed;

        // AI settings
        BrainClientType = defaults.BrainClientType;
        OllamaUrl = defaults.OllamaUrl;
        OllamaModel = defaults.OllamaModel;
    }

    #endregion
}

#region Option Models

/// <summary>
/// Represents a personality type option.
/// </summary>
public sealed record PersonalityOption(string Value, string DisplayName, string Description);

/// <summary>
/// Represents a speech style option.
/// </summary>
public sealed record SpeechStyleOption(string Value, string DisplayName, string Description);

/// <summary>
/// Represents a bubble style option.
/// </summary>
public sealed record BubbleStyleOption(string Value, string DisplayName, string Description);

/// <summary>
/// Represents an accent color option.
/// </summary>
public sealed record AccentColorOption(string HexColor, string Name, string Emoji);

/// <summary>
/// Represents a brain client type option.
/// </summary>
public sealed record BrainClientOption(string Value, string DisplayName, string Description);

/// <summary>
/// Represents an Ollama model option.
/// </summary>
public sealed record OllamaModelOption(string Value, string DisplayName, string Description);

#endregion
