using System.Windows;
using Youmii.Core.Interfaces;
using Youmii.Core.Models;
using Youmii.Features.Games.Chess.ViewModels;
using Youmii.Features.Games.Chess.Views;
using Youmii.Features.Games.ViewModels;
using Youmii.Features.Games.Views;

namespace Youmii.Features.Games.Services;

/// <summary>
/// Coordinates game operations including displaying the game selection window and launching games.
/// </summary>
public sealed class GamesCoordinator
{
    private readonly GameService _gameService;

    public GamesCoordinator()
    {
        _gameService = new GameService();
    }

    /// <summary>
    /// Opens the games selection dialog and returns the selected game ID if any.
    /// </summary>
    /// <returns>The selected game ID, or null if cancelled.</returns>
    public string? OpenGamesDialog()
    {
        var gamesVm = new GamesViewModel(_gameService);
        var gamesWindow = new GamesWindow();
        
        // Set owner to main window for proper modal behavior
        gamesWindow.Owner = Application.Current.MainWindow;
        
        gamesWindow.SetViewModel(gamesVm);

        string? selectedGameId = null;
        gamesVm.GameLaunched += (_, id) => selectedGameId = id;

        var result = gamesWindow.ShowDialog();

        if (result == true && !string.IsNullOrEmpty(selectedGameId))
        {
            LaunchGame(selectedGameId);
            return selectedGameId;
        }

        return null;
    }

    /// <summary>
    /// Launches a specific game by ID.
    /// </summary>
    public void LaunchGame(string gameId)
    {
        switch (gameId)
        {
            case "chess":
                LaunchChess();
                break;
            // Add more games here as they're implemented
            default:
                // Game not yet implemented
                break;
        }
    }

    private static void LaunchChess()
    {
        var chessVm = new ChessGameViewModel();
        var chessWindow = new ChessGameWindow();
        chessWindow.SetViewModel(chessVm);
        chessWindow.Show();
    }
}

/// <summary>
/// Default implementation of the game service with available games.
/// </summary>
public sealed class GameService : IGameService
{
    private readonly List<GameInfo> _games;

    public GameService()
    {
        _games = CreateDefaultGames();
    }

    public IReadOnlyList<GameInfo> GetAllGames() => _games;

    public IReadOnlyList<GameInfo> GetGamesByCategory(string category)
    {
        return _games.Where(g => g.Category == category).ToList();
    }

    public GameInfo? GetGameById(string gameId)
    {
        return _games.FirstOrDefault(g => g.Id == gameId);
    }

    public async Task<bool> LaunchGameAsync(string gameId)
    {
        var game = GetGameById(gameId);
        if (game?.IsAvailable != true) return false;

        // Games are launched synchronously via the coordinator
        await Task.CompletedTask;
        return true;
    }

    public IReadOnlyList<string> GetCategories()
    {
        return _games.Select(g => g.Category).Distinct().ToList();
    }

    private static List<GameInfo> CreateDefaultGames()
    {
        return new List<GameInfo>
        {
            // Available Games
            new()
            {
                Id = "chess",
                Name = "Chess",
                Description = "Classic strategy board game",
                Icon = "\u265E", // Knight symbol
                Category = "Board",
                IsAvailable = true,
                Difficulty = 4,
                AccentColor = "#FF9C27B0"
            },

            // Coming Soon Games
            new()
            {
                Id = "checkers",
                Name = "Checkers",
                Description = "Simple and fun board game",
                Icon = "\u25CF", // Circle
                Category = "Board",
                IsAvailable = false,
                Difficulty = 2,
                AccentColor = "#FFE91E63"
            },
            new()
            {
                Id = "tictactoe",
                Name = "Tic Tac Toe",
                Description = "Quick and easy classic",
                Icon = "#",
                Category = "Board",
                IsAvailable = false,
                Difficulty = 1,
                AccentColor = "#FF2196F3"
            },
            new()
            {
                Id = "memory",
                Name = "Memory Match",
                Description = "Test your memory!",
                Icon = "\u2665", // Heart
                Category = "Puzzle",
                IsAvailable = false,
                Difficulty = 2,
                AccentColor = "#FFFF5722"
            },
            new()
            {
                Id = "sudoku",
                Name = "Sudoku",
                Description = "Number puzzle challenge",
                Icon = "9",
                Category = "Puzzle",
                IsAvailable = false,
                Difficulty = 3,
                AccentColor = "#FF4CAF50"
            },
            new()
            {
                Id = "solitaire",
                Name = "Solitaire",
                Description = "Classic card game",
                Icon = "\u2660", // Spade
                Category = "Card",
                IsAvailable = false,
                Difficulty = 2,
                AccentColor = "#FF00BCD4"
            },
            new()
            {
                Id = "snake",
                Name = "Snake",
                Description = "Classic arcade game",
                Icon = "~",
                Category = "Arcade",
                IsAvailable = false,
                Difficulty = 2,
                AccentColor = "#FF8BC34A"
            },
            new()
            {
                Id = "breakout",
                Name = "Breakout",
                Description = "Brick breaking fun",
                Icon = "\u25A0", // Square
                Category = "Arcade",
                IsAvailable = false,
                Difficulty = 3,
                AccentColor = "#FFFFC107"
            }
        };
    }
}
