namespace Youmii.Features.Games.MemoryMatch.Models;

/// <summary>
/// Represents a card in the Memory Match game.
/// </summary>
public sealed class MemoryCard
{
    public int Id { get; init; }
    public int PairId { get; init; }
    public string Symbol { get; init; } = string.Empty;
    public string Color { get; init; } = "#FF9C27B0";
    public bool IsFlipped { get; set; }
    public bool IsMatched { get; set; }
}

/// <summary>
/// Available card symbols for the game.
/// </summary>
public static class CardSymbols
{
    public static readonly (string Symbol, string Color)[] Pairs =
    [
        ("??", "#FFFFC107"), // Star
        ("??", "#FFE91E63"), // Heart
        ("??", "#FF9C27B0"), // Moon
        ("??", "#FFFF5722"), // Fire
        ("??", "#FF00BCD4"), // Diamond
        ("??", "#FF4CAF50"), // Clover
        ("?", "#FFFF9800"), // Lightning
        ("??", "#FF2196F3"), // Music
    ];
}
