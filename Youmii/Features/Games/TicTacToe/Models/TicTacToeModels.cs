namespace Youmii.Features.Games.TicTacToe.Models;

/// <summary>
/// Represents a player in Tic Tac Toe.
/// </summary>
public enum TicTacToePlayer
{
    None,
    X,
    O
}

/// <summary>
/// Represents the game state.
/// </summary>
public enum TicTacToeGameState
{
    Playing,
    XWins,
    OWins,
    Draw
}

/// <summary>
/// Represents a cell on the Tic Tac Toe board.
/// </summary>
public sealed class TicTacToeCell
{
    public int Row { get; init; }
    public int Column { get; init; }
    public TicTacToePlayer Player { get; set; } = TicTacToePlayer.None;
    public bool IsWinningCell { get; set; }

    public string Symbol => Player switch
    {
        TicTacToePlayer.X => "X",
        TicTacToePlayer.O => "O",
        _ => string.Empty
    };
}
