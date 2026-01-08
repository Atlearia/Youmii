using System.Windows.Threading;
using Youmii.Core.Interfaces;
using Youmii.Core.Models;
using Youmii.Core.Services;
using Youmii.Infrastructure;
using Youmii.Views;

namespace Youmii.ViewModels;

/// <summary>
/// Main ViewModel for the overlay window.
/// </summary>
public sealed class MainViewModel : ViewModelBase, IDisposable
{
    private readonly ServiceFactory _serviceFactory;
    private readonly IConversationService _conversationService;
    private readonly IBrainClient _brainClient;
    private readonly IdleMessageService _idleMessageService;
    private readonly SettingsService _settingsService;
    private readonly DispatcherTimer _autoHideTimer;
    private readonly DispatcherTimer _idleMessageTimer;

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
        _serviceFactory = new ServiceFactory();
        _conversationService = _serviceFactory.CreateConversationService();
        _brainClient = _serviceFactory.CreateBrainClient();
        _idleMessageService = new IdleMessageService();
        _settingsService = new SettingsService();

        _autoHideTimer = new DispatcherTimer();
        _autoHideTimer.Tick += (_, _) =>
        {
            IsBubbleVisible = false;
            _autoHideTimer.Stop();
        };

        // Initialize idle message timer
        _idleMessageTimer = new DispatcherTimer();
        _idleMessageTimer.Tick += OnIdleMessageTimerTick;

        // Initialize RadialMenu with service
        var radialMenuService = new RadialMenuService();
        RadialMenu = new RadialMenuViewModel(radialMenuService);

        SendCommand = new AsyncRelayCommand(SendMessageAsync, () => CanSend);
        ToggleInputCommand = new RelayCommand(ToggleInput);
        ToggleOverlayCommand = new RelayCommand(ToggleOverlay);
        ClearHistoryCommand = new AsyncRelayCommand(ClearHistoryAsync);

        // Initialize database and settings
        _ = InitializeAsync();
    }

    /// <summary>
    /// Gets the radial menu ViewModel.
    /// </summary>
    public RadialMenuViewModel RadialMenu { get; }

    /// <summary>
    /// Gets the settings service for external access.
    /// </summary>
    public ISettingsService SettingsService => _settingsService;

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
        set => SetProperty(ref _isInputVisible, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetProperty(ref _isLoading, value))
            {
                ((AsyncRelayCommand)SendCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsOverlayVisible
    {
        get => _isOverlayVisible;
        set => SetProperty(ref _isOverlayVisible, value);
    }

    public bool CanSend => !string.IsNullOrWhiteSpace(UserInput) && !IsLoading;

    public AsyncRelayCommand SendCommand { get; }
    public RelayCommand ToggleInputCommand { get; }
    public RelayCommand ToggleOverlayCommand { get; }
    public AsyncRelayCommand ClearHistoryCommand { get; }

    /// <summary>
    /// Handles when a radial menu item is selected.
    /// Called from MainWindow after the item selection event.
    /// </summary>
    public void HandleRadialMenuItemSelected(RadialMenuItem item)
    {
        ResetIdleTimer();
        
        // Handle specific menu item actions
        switch (item.Id)
        {
            case "chat":
                IsInputVisible = true;
                break;
            case "settings":
                OpenSettingsWindow();
                break;
            // Other menu items are placeholders for now
            default:
                BubbleText = $"{item.Icon} {item.Label} - Coming soon!";
                IsBubbleVisible = true;
                ResetAutoHideTimer();
                break;
        }
    }

    /// <summary>
    /// Opens the settings window and applies any changes.
    /// </summary>
    private void OpenSettingsWindow()
    {
        var settingsVm = new SettingsViewModel(_settingsService);
        var settingsWindow = new SettingsWindow();
        settingsWindow.SetViewModel(settingsVm);
        
        var result = settingsWindow.ShowDialog();
        
        if (result == true)
        {
            ApplySettings();
            BubbleText = "Settings saved! ??";
        }
        else
        {
            BubbleText = "Settings unchanged~";
        }
        
        IsBubbleVisible = true;
        ResetAutoHideTimer();
    }

    /// <summary>
    /// Applies current settings to all relevant services and properties.
    /// </summary>
    private void ApplySettings()
    {
        var settings = _settingsService.CurrentSettings;
        
        // Update idle message service
        _idleMessageService.Configure(
            settings.IdleMinIntervalSeconds,
            settings.IdleMaxIntervalSeconds,
            settings.IdleMessagesEnabled
        );
        
        // Update auto-hide timer
        _autoHideTimer.Interval = TimeSpan.FromSeconds(settings.BubbleDisplaySeconds);
        
        // Update visual properties
        CharacterOpacity = settings.CharacterOpacity;
        CharacterScale = settings.CharacterScale;
        AlwaysOnTop = settings.AlwaysOnTop;
        
        // Reset idle timer with new settings
        ResetIdleTimer();
    }

    private async Task InitializeAsync()
    {
        try
        {
            // Load settings first
            await _settingsService.LoadAsync();
            ApplySettings();
            
            await _serviceFactory.InitializeAsync();
            
            // Start idle timer
            ResetIdleTimer();
            
            // Show welcome message with character name
            var name = _settingsService.CurrentSettings.CharacterName;
            BubbleText = $"Hello! I'm {name}! Hold click on me for options!";
            IsBubbleVisible = true;
            ResetAutoHideTimer();
        }
        catch (Exception ex)
        {
            BubbleText = $"Error initializing: {ex.Message}";
            IsBubbleVisible = true;
        }
    }

    private async Task SendMessageAsync()
    {
        if (!CanSend) return;

        ResetIdleTimer();
        
        var input = UserInput.Trim();
        UserInput = string.Empty;
        IsLoading = true;

        try
        {
            // Prepare request (saves user message, extracts facts)
            var result = await _conversationService.PrepareRequestAsync(input);

            // Show "thinking" state
            BubbleText = "...";
            IsBubbleVisible = true;

            // Send to brain
            var response = await _brainClient.SendMessageAsync(result.Request);

            // Save response
            await _conversationService.SaveResponseAsync(response.Reply);

            // Display reply
            BubbleText = response.Reply;
            ResetAutoHideTimer();
        }
        catch (Exception ex)
        {
            BubbleText = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ToggleInput()
    {
        ResetIdleTimer();
        
        IsInputVisible = !IsInputVisible;
        
        if (IsInputVisible)
        {
            // Stop auto-hide while input is open
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

    private async Task ClearHistoryAsync()
    {
        ResetIdleTimer();
        
        await _conversationService.ClearHistoryAsync();
        BubbleText = "Conversation cleared!";
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

    /// <summary>
    /// Resets the idle timer. Call this on any user interaction.
    /// </summary>
    public void ResetIdleTimer()
    {
        _idleMessageTimer.Stop();
        
        if (_idleMessageService.IsEnabled)
        {
            _idleMessageTimer.Interval = _idleMessageService.GetRandomInterval();
            _idleMessageTimer.Start();
        }
    }

    private void OnIdleMessageTimerTick(object? sender, EventArgs e)
    {
        // Don't show idle messages when user is actively interacting or if disabled
        if (IsLoading || IsInputVisible || !_idleMessageService.IsEnabled)
        {
            ResetIdleTimer();
            return;
        }

        // Show random idle message
        BubbleText = _idleMessageService.GetRandomMessage();
        IsBubbleVisible = true;
        ResetAutoHideTimer();

        // Schedule next idle message with new random interval
        ResetIdleTimer();
    }

    public void Dispose()
    {
        _autoHideTimer.Stop();
        _idleMessageTimer.Stop();
        _serviceFactory.Dispose();
    }
}
