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

        SaveCommand = new RelayCommand(Save, () => HasChanges);
        CancelCommand = new RelayCommand(Cancel);
        ResetToDefaultsCommand = new RelayCommand(ResetToDefaults);
    }

    #region Bindable Properties

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
        // Apply changes to the service
        _settingsService.CurrentSettings.CharacterName = _editingSettings.CharacterName;
        _settingsService.CurrentSettings.IdleMinIntervalSeconds = _editingSettings.IdleMinIntervalSeconds;
        _settingsService.CurrentSettings.IdleMaxIntervalSeconds = _editingSettings.IdleMaxIntervalSeconds;
        _settingsService.CurrentSettings.IdleMessagesEnabled = _editingSettings.IdleMessagesEnabled;
        _settingsService.CurrentSettings.BubbleDisplaySeconds = _editingSettings.BubbleDisplaySeconds;
        _settingsService.CurrentSettings.AlwaysOnTop = _editingSettings.AlwaysOnTop;
        _settingsService.CurrentSettings.CharacterOpacity = _editingSettings.CharacterOpacity;
        _settingsService.CurrentSettings.SoundEffectsEnabled = _editingSettings.SoundEffectsEnabled;
        _settingsService.CurrentSettings.StartWithWindows = _editingSettings.StartWithWindows;
        _settingsService.CurrentSettings.CharacterScale = _editingSettings.CharacterScale;

        // Save to disk asynchronously
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
    }

    #endregion
}
