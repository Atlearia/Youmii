namespace Youmii.Features.Games.Chess.Models;

/// <summary>
/// Types of chess pieces.
/// </summary>
public enum ChessPieceType
{
    Pawn,
    Rook,
    Knight,
    Bishop,
    Queen,
    King
}

/// <summary>
/// Represents a chess piece.
/// </summary>
public sealed class ChessPiece
{
    public ChessPieceType Type { get; init; }
    public bool IsWhite { get; init; }
}

/// <summary>
/// Represents a move in the game for undo functionality.
/// </summary>
public sealed class ChessMove
{
    public int FromRow { get; init; }
    public int FromCol { get; init; }
    public int ToRow { get; init; }
    public int ToCol { get; init; }
    public ChessPiece? MovedPiece { get; init; }
    public ChessPiece? CapturedPiece { get; init; }
}

/// <summary>
/// Helper class for chess piece Unicode symbols.
/// </summary>
public static class ChessPieceSymbols
{
    public static string GetSymbol(ChessPieceType type, bool isWhite)
    {
        return (type, isWhite) switch
        {
            (ChessPieceType.King, true) => "\u2654",
            (ChessPieceType.Queen, true) => "\u2655",
            (ChessPieceType.Rook, true) => "\u2656",
            (ChessPieceType.Bishop, true) => "\u2657",
            (ChessPieceType.Knight, true) => "\u2658",
            (ChessPieceType.Pawn, true) => "\u2659",
            (ChessPieceType.King, false) => "\u265A",
            (ChessPieceType.Queen, false) => "\u265B",
            (ChessPieceType.Rook, false) => "\u265C",
            (ChessPieceType.Bishop, false) => "\u265D",
            (ChessPieceType.Knight, false) => "\u265E",
            (ChessPieceType.Pawn, false) => "\u265F",
            _ => string.Empty
        };
    }
}
