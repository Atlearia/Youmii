using System.Windows.Threading;
using Youmii.Core.Interfaces;
using Youmii.Core.Models;
using Youmii.Core.Services;
using Youmii.Infrastructure;

namespace Youmii.ViewModels;

/// <summary>
/// Main ViewModel for the overlay window.
/// </summary>
public sealed class MainViewModel : ViewModelBase, IDisposable
{
    private readonly ServiceFactory _serviceFactory;
    private readonly IConversationService _conversationService;
    private readonly IBrainClient _brainClient;
    private readonly DispatcherTimer _autoHideTimer;
    private readonly int _autoHideSeconds;

    private string _userInput = string.Empty;
    private string _bubbleText = string.Empty;
    private bool _isBubbleVisible;
    private bool _isInputVisible;
    private bool _isLoading;
    private bool _isOverlayVisible = true;
    private bool _isCharacterDimmed;

    public MainViewModel()
    {
        _serviceFactory = new ServiceFactory();
        _conversationService = _serviceFactory.CreateConversationService();
        _brainClient = _serviceFactory.CreateBrainClient();
        _autoHideSeconds = _serviceFactory.Settings.BubbleAutoHideSeconds;

        _autoHideTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(_autoHideSeconds)
        };
        _autoHideTimer.Tick += (_, _) =>
        {
            IsBubbleVisible = false;
            _autoHideTimer.Stop();
        };

        // Initialize RadialMenu with service
        var radialMenuService = new RadialMenuService();
        RadialMenu = new RadialMenuViewModel(radialMenuService);

        SendCommand = new AsyncRelayCommand(SendMessageAsync, () => CanSend);
        ToggleInputCommand = new RelayCommand(ToggleInput);
        ToggleOverlayCommand = new RelayCommand(ToggleOverlay);
        ClearHistoryCommand = new AsyncRelayCommand(ClearHistoryAsync);

        // Initialize database
        _ = InitializeAsync();
    }

    /// <summary>
    /// Gets the radial menu ViewModel.
    /// </summary>
    public RadialMenuViewModel RadialMenu { get; }

    /// <summary>
    /// Gets or sets whether the character image is dimmed (when radial menu is open).
    /// </summary>
    public bool IsCharacterDimmed
    {
        get => _isCharacterDimmed;
        set => SetProperty(ref _isCharacterDimmed, value);
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
        // Handle specific menu item actions
        switch (item.Id)
        {
            case "chat":
                IsInputVisible = true;
                break;
            // Other menu items are placeholders for now
            default:
                BubbleText = $"{item.Icon} {item.Label} - Coming soon!";
                IsBubbleVisible = true;
                ResetAutoHideTimer();
                break;
        }
    }

    private async Task InitializeAsync()
    {
        try
        {
            await _serviceFactory.InitializeAsync();
            
            // Show welcome message
            BubbleText = "Hello! Hold click on me for options!";
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

    public void Dispose()
    {
        _autoHideTimer.Stop();
        _serviceFactory.Dispose();
    }
}
