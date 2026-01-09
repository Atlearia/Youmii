using Youmii.Core.Models;

namespace Youmii.Core.Interfaces;

/// <summary>
/// Service for managing available games.
/// </summary>
public interface IGameService
{
    /// <summary>
    /// Gets all available games.
    /// </summary>
    IReadOnlyList<GameInfo> GetAllGames();

    /// <summary>
    /// Gets games filtered by category.
    /// </summary>
    IReadOnlyList<GameInfo> GetGamesByCategory(string category);

    /// <summary>
    /// Gets a specific game by ID.
    /// </summary>
    GameInfo? GetGameById(string gameId);

    /// <summary>
    /// Launches a game by ID.
    /// </summary>
    Task<bool> LaunchGameAsync(string gameId);

    /// <summary>
    /// Gets all available game categories.
    /// </summary>
    IReadOnlyList<string> GetCategories();
}
