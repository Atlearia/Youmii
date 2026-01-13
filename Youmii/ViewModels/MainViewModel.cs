using System.Windows.Threading;
using Youmii.Core.Interfaces;
using Youmii.Core.Models;
using Youmii.Core.Services;
using Youmii.Features.Chat.Services;
using Youmii.Features.Games.Services;
using Youmii.Features.IdleMessages.Services;
using Youmii.Features.Settings.Services;
using Youmii.Infrastructure;

namespace Youmii.ViewModels;

/// <summary>
/// Main ViewModel for the overlay window.
/// Coordinates between feature modules and manages UI state.
/// </summary>
public sealed class MainViewModel : ViewModelBase, IDisposable
{
    private readonly ServiceFactory _serviceFactory;
    private readonly SettingsCoordinator _settingsCoordinator;
    private readonly ChatCoordinator _chatCoordinator;
    private readonly IdleMessageCoordinator _idleMessageCoordinator;
    private readonly GamesCoordinator _gamesCoordinator;
    private readonly DispatcherTimer _autoHideTimer;
    private readonly IBrainClient _brainClient;

    private string _userInput = string.Empty;
    private string _bubbleText = string.Empty;
    private bool _isBubbleVisible;
    private bool _isInputVisible;
    private bool _isLoading;
    private bool _isOverlayVisible = true;
    private bool _isCharacterDimmed;
    private double _characterOpacity = 1.0;
    private double _characterScale = 1.0;
    private bool _alwaysOnTop = true;

    public MainViewModel()
    {
        // Initialize infrastructure
        _serviceFactory = new ServiceFactory();
        
        // Get brain client and store reference
        _brainClient = _serviceFactory.CreateBrainClient();
        
        // Initialize feature coordinators
        _settingsCoordinator = new SettingsCoordinator();
        _chatCoordinator = new ChatCoordinator(
            _serviceFactory.CreateConversationService(),
            _brainClient
        );
        _idleMessageCoordinator = new IdleMessageCoordinator();
        _gamesCoordinator = new GamesCoordinator();

        // Subscribe to coordinator events
        _settingsCoordinator.SettingsApplied += OnSettingsApplied;
        _chatCoordinator.MessageSending += OnMessageSending;
        _chatCoordinator.MessageReceived += OnMessageReceived;
        _chatCoordinator.MessageFailed += OnMessageFailed;
        _idleMessageCoordinator.IdleMessageTriggered += OnIdleMessageTriggered;

        // Initialize auto-hide timer for bubble
        _autoHideTimer = new DispatcherTimer();
        _autoHideTimer.Tick += OnAutoHideTimerTick;

        // Initialize RadialMenu
        var radialMenuService = new RadialMenuService();
        RadialMenu = new RadialMenuViewModel(radialMenuService);

        // Initialize commands
        SendCommand = new AsyncRelayCommand(SendMessageAsync, () => CanSend);
        ToggleInputCommand = new RelayCommand(ToggleInput);
        ToggleOverlayCommand = new RelayCommand(ToggleOverlay);
        ClearHistoryCommand = new AsyncRelayCommand(ClearHistoryAsync);

        // Initialize asynchronously
        _ = InitializeAsync();
    }

    #region Properties

    /// <summary>
    /// Gets the radial menu ViewModel.
    /// </summary>
    public RadialMenuViewModel RadialMenu { get; }

    /// <summary>
    /// Gets the settings service for external access (used by MainWindow for animations).
    /// </summary>
    public ISettingsService SettingsService => _settingsCoordinator.SettingsService;

    /// <summary>
    /// Gets or sets whether the character image is dimmed (when radial menu is open).
    /// </summary>
    public bool IsCharacterDimmed
    {
        get => _isCharacterDimmed;
        set => SetProperty(ref _isCharacterDimmed, value);
    }

    /// <summary>
    /// Gets or sets the character opacity from settings.
    /// </summary>
    public double CharacterOpacity
    {
        get => _characterOpacity;
        set => SetProperty(ref _characterOpacity, value);
    }

    /// <summary>
    /// Gets or sets the character scale from settings.
    /// </summary>
    public double CharacterScale
    {
        get => _characterScale;
        set => SetProperty(ref _characterScale, value);
    }

    /// <summary>
    /// Gets or sets whether the window should always be on top.
    /// </summary>
    public bool AlwaysOnTop
    {
        get => _alwaysOnTop;
        set => SetProperty(ref _alwaysOnTop, value);
    }

    public string UserInput
    {
        get => _userInput;
        set
        {
            if (SetProperty(ref _userInput, value))
            {
                ((AsyncRelayCommand)SendCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string BubbleText
    {
        get => _bubbleText;
        set => SetProperty(ref _bubbleText, value);
    }

    public bool IsBubbleVisible
    {
        get => _isBubbleVisible;
        set => SetProperty(ref _isBubbleVisible, value);
    }

    public bool IsInputVisible
    {
        get => _isInputVisible;
        set
        {
            if (SetProperty(ref _isInputVisible, value))
            {
                // Pause idle messages when input is visible
                _idleMessageCoordinator.IsPaused = value || IsLoading;
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetProperty(ref _isLoading, value))
            {
                ((AsyncRelayCommand)SendCommand).RaiseCanExecuteChanged();
                // Pause idle messages when loading
                _idleMessageCoordinator.IsPaused = value || IsInputVisible;
            }
        }
    }

    public bool IsOverlayVisible
    {
        get => _isOverlayVisible;
        set => SetProperty(ref _isOverlayVisible, value);
    }

    public bool CanSend => !string.IsNullOrWhiteSpace(UserInput) && !IsLoading;

    #endregion

    #region Commands

    public AsyncRelayCommand SendCommand { get; }
    public RelayCommand ToggleInputCommand { get; }
    public RelayCommand ToggleOverlayCommand { get; }
    public AsyncRelayCommand ClearHistoryCommand { get; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Handles when a radial menu item is selected.
    /// </summary>
    public void HandleRadialMenuItemSelected(RadialMenuItem item)
    {
        ResetIdleTimer();
        
        switch (item.Id)
        {
            case "chat":
                IsInputVisible = true;
                break;
            case "settings":
                OpenSettings();
                break;
            case "games":
                OpenGames();
                break;
            default:
                ShowBubble($"{item.Icon} {item.Label} - Coming soon!");
                break;
        }
    }

    /// <summary>
    /// Resets the idle timer. Call this on any user interaction.
    /// </summary>
    public void ResetIdleTimer()
    {
        _idleMessageCoordinator.ResetTimer();
    }

    #endregion

    #region Private Methods - Initialization

    private async Task InitializeAsync()
    {
        try
        {
            // Load and apply settings
            await _settingsCoordinator.LoadSettingsAsync();
            _settingsCoordinator.ApplySettings();
            
            // Initialize database
            await _serviceFactory.InitializeAsync();

            // Initialize brain client (detects Ollama availability for SmartBrainClient)
            if (_brainClient is Infrastructure.Brain.SmartBrainClient smartClient)
            {
                await smartClient.InitializeAsync();
            }
            
            // Start idle messages
            _idleMessageCoordinator.Start();
            
            // Show welcome message with AI backend info (now properly initialized)
            var name = _settingsCoordinator.CurrentSettings.CharacterName;
            ShowBubble($"Hello! I'm {name}! Using: {_brainClient.ClientName}");
        }
        catch (Exception ex)
        {
            ShowBubble($"Error initializing: {ex.Message}");
        }
    }

    #endregion

    #region Private Methods - Settings

    private void OpenSettings()
    {
        // Store current AI settings before opening dialog
        var previousBrainType = _settingsCoordinator.CurrentSettings.BrainClientType;
        var previousModel = _settingsCoordinator.CurrentSettings.OllamaModel;
        
        var saved = _settingsCoordinator.OpenSettingsDialog();
        
        if (saved)
        {
            var settings = _settingsCoordinator.CurrentSettings;
            
            // Check if AI settings changed
            if (settings.BrainClientType != previousBrainType || 
                settings.OllamaModel != previousModel)
            {
                // Build the "Now using" message
                var aiDescription = GetAiBackendDescription(settings.BrainClientType, settings.OllamaModel);
                ShowBubble($"Now using: {aiDescription}! ?? Restart app to apply changes~");
            }
            else
            {
                ShowBubble("Settings saved! ?");
            }
        }
        else
        {
            ShowBubble("Settings unchanged~");
        }
    }

    private static string GetAiBackendDescription(string brainClientType, string ollamaModel)
    {
        return brainClientType switch
        {
            "Auto" => $"Auto-detect (Ollama {ollamaModel})",
            "Ollama" => $"Ollama ({ollamaModel})",
            "Stub" => "Offline Mode",
            "Http" => "HTTP Server",
            _ => brainClientType
        };
    }

    private void OnSettingsApplied(object? sender, SettingsAppliedEventArgs e)
    {
        var settings = e.Settings;
        
        // Update idle message coordinator
        _idleMessageCoordinator.Configure(
            settings.IdleMinIntervalSeconds,
            settings.IdleMaxIntervalSeconds,
            settings.IdleMessagesEnabled
        );
        
        // Update auto-hide timer interval
        _autoHideTimer.Interval = TimeSpan.FromSeconds(settings.BubbleDisplaySeconds);
        
        // Update visual properties
        CharacterOpacity = settings.CharacterOpacity;
        CharacterScale = settings.CharacterScale;
        AlwaysOnTop = settings.AlwaysOnTop;
    }

    #endregion

    #region Private Methods - Chat

    private async Task SendMessageAsync()
    {
        if (!CanSend) return;

        ResetIdleTimer();
        
        var input = UserInput.Trim();
        UserInput = string.Empty;

        await _chatCoordinator.SendMessageAsync(input);
    }

    private void OnMessageSending(object? sender, EventArgs e)
    {
        IsLoading = true;
        ShowBubble("...");
    }

    private void OnMessageReceived(object? sender, ChatResponseEventArgs e)
    {
        IsLoading = false;
        ShowBubble(e.Response);
    }

    private void OnMessageFailed(object? sender, ChatErrorEventArgs e)
    {
        IsLoading = false;
        ShowBubble($"Error: {e.ErrorMessage}");
    }

    private async Task ClearHistoryAsync()
    {
        ResetIdleTimer();
        await _chatCoordinator.ClearHistoryAsync();
        ShowBubble("Conversation cleared!");
    }

    #endregion

    #region Private Methods - Games

    private void OpenGames()
    {
        // Close the radial menu first
        RadialMenu.Hide();
        
        // Use dispatcher to allow UI to update before opening modal
        System.Windows.Application.Current.Dispatcher.BeginInvoke(
            System.Windows.Threading.DispatcherPriority.Background,
            new Action(() =>
            {
                var selectedGame = _gamesCoordinator.OpenGamesDialog();
                if (!string.IsNullOrEmpty(selectedGame))
                {
                    ShowBubble("Let's play! Good luck!");
                }
                else
                {
                    ShowBubble("Come back when you want to play~");
                }
            }));
    }

    #endregion

    #region Private Methods - UI

    private void ToggleInput()
    {
        ResetIdleTimer();
        IsInputVisible = !IsInputVisible;
        
        if (IsInputVisible)
        {
            _autoHideTimer.Stop();
        }
        else if (IsBubbleVisible)
        {
            ResetAutoHideTimer();
        }
    }

    private void ToggleOverlay()
    {
        IsOverlayVisible = !IsOverlayVisible;
    }

    private void ShowBubble(string text)
    {
        BubbleText = text;
        IsBubbleVisible = true;
        ResetAutoHideTimer();
    }

    private void ResetAutoHideTimer()
    {
        _autoHideTimer.Stop();
        if (!IsInputVisible)
        {
            _autoHideTimer.Start();
        }
    }

    private void OnAutoHideTimerTick(object? sender, EventArgs e)
    {
        IsBubbleVisible = false;
        _autoHideTimer.Stop();
    }

    private void OnIdleMessageTriggered(object? sender, IdleMessageEventArgs e)
    {
        ShowBubble(e.Message);
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        // Unsubscribe from events
        _settingsCoordinator.SettingsApplied -= OnSettingsApplied;
        _chatCoordinator.MessageSending -= OnMessageSending;
        _chatCoordinator.MessageReceived -= OnMessageReceived;
        _chatCoordinator.MessageFailed -= OnMessageFailed;
        _idleMessageCoordinator.IdleMessageTriggered -= OnIdleMessageTriggered;

        // Stop timers
        _autoHideTimer.Stop();

        // Dispose coordinators
        _settingsCoordinator.Dispose();
        _idleMessageCoordinator.Dispose();
        _serviceFactory.Dispose();
    }

    #endregion
}
