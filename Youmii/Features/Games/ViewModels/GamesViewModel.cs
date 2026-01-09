using System.Collections.ObjectModel;
using System.Windows.Input;
using Youmii.Core.Interfaces;
using Youmii.Core.Models;
using Youmii.ViewModels;

namespace Youmii.Features.Games.ViewModels;

/// <summary>
/// ViewModel for the Games selection window.
/// </summary>
public sealed class GamesViewModel : ViewModelBase
{
    private readonly IGameService _gameService;
    private string _selectedCategory = "All";
    private bool _isAllSelected = true;

    public GamesViewModel(IGameService gameService)
    {
        _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));

        AllGames = new ObservableCollection<GameInfo>(_gameService.GetAllGames());
        FilteredGames = new ObservableCollection<GameInfo>(AllGames);

        SelectCategoryCommand = new RelayCommand<string>(SelectCategory);
        LaunchGameCommand = new RelayCommand<string>(LaunchGame, CanLaunchGame);
    }

    #region Properties

    /// <summary>
    /// Gets all available games.
    /// </summary>
    public ObservableCollection<GameInfo> AllGames { get; }

    /// <summary>
    /// Gets the filtered games based on selected category.
    /// </summary>
    public ObservableCollection<GameInfo> FilteredGames { get; }

    /// <summary>
    /// Gets the currently selected category.
    /// </summary>
    public string SelectedCategory
    {
        get => _selectedCategory;
        private set
        {
            if (SetProperty(ref _selectedCategory, value))
            {
                IsAllSelected = value == "All";
                FilterGames();
            }
        }
    }

    /// <summary>
    /// Gets whether "All" category is selected.
    /// </summary>
    public bool IsAllSelected
    {
        get => _isAllSelected;
        private set => SetProperty(ref _isAllSelected, value);
    }

    #endregion

    #region Commands

    public ICommand SelectCategoryCommand { get; }
    public ICommand LaunchGameCommand { get; }

    #endregion

    #region Events

    /// <summary>
    /// Event raised when the window should close.
    /// </summary>
    public event EventHandler? RequestClose;

    /// <summary>
    /// Event raised when a game is launched.
    /// </summary>
    public event EventHandler<string>? GameLaunched;

    #endregion

    #region Private Methods

    private void SelectCategory(string? category)
    {
        if (string.IsNullOrEmpty(category)) return;
        SelectedCategory = category;
    }

    private void FilterGames()
    {
        FilteredGames.Clear();

        var games = SelectedCategory == "All"
            ? AllGames
            : AllGames.Where(g => g.Category == SelectedCategory);

        foreach (var game in games)
        {
            FilteredGames.Add(game);
        }
    }

    private bool CanLaunchGame(string? gameId)
    {
        if (string.IsNullOrEmpty(gameId)) return false;
        var game = _gameService.GetGameById(gameId);
        return game?.IsAvailable == true;
    }

    private void LaunchGame(string? gameId)
    {
        if (string.IsNullOrEmpty(gameId)) return;

        var game = _gameService.GetGameById(gameId);
        if (game?.IsAvailable != true) return;

        GameLaunched?.Invoke(this, gameId);
    }

    #endregion
}
