namespace Youmii.Core.Models;

/// <summary>
/// Represents information about a game available to play.
/// </summary>
public sealed class GameInfo
{
    /// <summary>
    /// Gets or sets the unique identifier for the game.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets or sets the display name of the game.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets a brief description of the game.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets or sets the icon/emoji for the game.
    /// </summary>
    public required string Icon { get; init; }

    /// <summary>
    /// Gets or sets the category of the game (e.g., "Board", "Puzzle", "Card").
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Gets or sets whether the game is currently available to play.
    /// </summary>
    public bool IsAvailable { get; init; } = true;

    /// <summary>
    /// Gets or sets the difficulty level (1-5).
    /// </summary>
    public int Difficulty { get; init; } = 3;

    /// <summary>
    /// Gets or sets the accent color for the game card (hex).
    /// </summary>
    public string AccentColor { get; init; } = "#FFE91E63";
}
