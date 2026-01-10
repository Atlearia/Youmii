using Youmii.Features.Games.Chess.Models;
using Youmii.Features.Games.Chess.ViewModels;

namespace Youmii.Features.Games.Chess.Engine;

/// <summary>
/// Evaluates chess positions for the AI engine.
/// </summary>
public sealed class MoveEvaluator
{
    // Piece values (centipawns)
    private static readonly Dictionary<ChessPieceType, int> PieceValues = new()
    {
        { ChessPieceType.Pawn, 100 },
        { ChessPieceType.Knight, 320 },
        { ChessPieceType.Bishop, 330 },
        { ChessPieceType.Rook, 500 },
        { ChessPieceType.Queen, 900 },
        { ChessPieceType.King, 20000 }
    };

    // Piece-square tables for positional evaluation (from white's perspective)
    private static readonly int[,] PawnTable = {
        {  0,  0,  0,  0,  0,  0,  0,  0 },
        { 50, 50, 50, 50, 50, 50, 50, 50 },
        { 10, 10, 20, 30, 30, 20, 10, 10 },
        {  5,  5, 10, 25, 25, 10,  5,  5 },
        {  0,  0,  0, 20, 20,  0,  0,  0 },
        {  5, -5,-10,  0,  0,-10, -5,  5 },
        {  5, 10, 10,-20,-20, 10, 10,  5 },
        {  0,  0,  0,  0,  0,  0,  0,  0 }
    };

    private static readonly int[,] KnightTable = {
        { -50,-40,-30,-30,-30,-30,-40,-50 },
        { -40,-20,  0,  0,  0,  0,-20,-40 },
        { -30,  0, 10, 15, 15, 10,  0,-30 },
        { -30,  5, 15, 20, 20, 15,  5,-30 },
        { -30,  0, 15, 20, 20, 15,  0,-30 },
        { -30,  5, 10, 15, 15, 10,  5,-30 },
        { -40,-20,  0,  5,  5,  0,-20,-40 },
        { -50,-40,-30,-30,-30,-30,-40,-50 }
    };

    private static readonly int[,] BishopTable = {
        { -20,-10,-10,-10,-10,-10,-10,-20 },
        { -10,  0,  0,  0,  0,  0,  0,-10 },
        { -10,  0,  5, 10, 10,  5,  0,-10 },
        { -10,  5,  5, 10, 10,  5,  5,-10 },
        { -10,  0, 10, 10, 10, 10,  0,-10 },
        { -10, 10, 10, 10, 10, 10, 10,-10 },
        { -10,  5,  0,  0,  0,  0,  5,-10 },
        { -20,-10,-10,-10,-10,-10,-10,-20 }
    };

    private static readonly int[,] RookTable = {
        {  0,  0,  0,  0,  0,  0,  0,  0 },
        {  5, 10, 10, 10, 10, 10, 10,  5 },
        { -5,  0,  0,  0,  0,  0,  0, -5 },
        { -5,  0,  0,  0,  0,  0,  0, -5 },
        { -5,  0,  0,  0,  0,  0,  0, -5 },
        { -5,  0,  0,  0,  0,  0,  0, -5 },
        { -5,  0,  0,  0,  0,  0,  0, -5 },
        {  0,  0,  0,  5,  5,  0,  0,  0 }
    };

    private static readonly int[,] QueenTable = {
        { -20,-10,-10, -5, -5,-10,-10,-20 },
        { -10,  0,  0,  0,  0,  0,  0,-10 },
        { -10,  0,  5,  5,  5,  5,  0,-10 },
        {  -5,  0,  5,  5,  5,  5,  0, -5 },
        {   0,  0,  5,  5,  5,  5,  0, -5 },
        { -10,  5,  5,  5,  5,  5,  0,-10 },
        { -10,  0,  5,  0,  0,  0,  0,-10 },
        { -20,-10,-10, -5, -5,-10,-10,-20 }
    };

    private static readonly int[,] KingMiddleGameTable = {
        { -30,-40,-40,-50,-50,-40,-40,-30 },
        { -30,-40,-40,-50,-50,-40,-40,-30 },
        { -30,-40,-40,-50,-50,-40,-40,-30 },
        { -30,-40,-40,-50,-50,-40,-40,-30 },
        { -20,-30,-30,-40,-40,-30,-30,-20 },
        { -10,-20,-20,-20,-20,-20,-20,-10 },
        {  20, 20,  0,  0,  0,  0, 20, 20 },
        {  20, 30, 10,  0,  0, 10, 30, 20 }
    };

    /// <summary>
    /// Evaluates the current board position.
    /// </summary>
    /// <param name="squares">The board squares.</param>
    /// <param name="isWhitePerspective">True to evaluate from white's perspective.</param>
    /// <returns>Score in centipawns (positive = good for the perspective side).</returns>
    public int EvaluatePosition(IReadOnlyList<ChessSquareViewModel> squares, bool isWhitePerspective)
    {
        int whiteScore = 0;
        int blackScore = 0;

        foreach (var square in squares)
        {
            if (square.Piece == null) continue;

            var piece = square.Piece;
            var materialValue = PieceValues[piece.Type];
            var positionalValue = GetPositionalValue(piece.Type, square.Row, square.Column, piece.IsWhite);

            if (piece.IsWhite)
            {
                whiteScore += materialValue + positionalValue;
            }
            else
            {
                blackScore += materialValue + positionalValue;
            }
        }

        // Add mobility bonus (simplified)
        whiteScore += CountMobility(squares, true) * 2;
        blackScore += CountMobility(squares, false) * 2;

        var totalScore = whiteScore - blackScore;
        return isWhitePerspective ? totalScore : -totalScore;
    }

    private int GetPositionalValue(ChessPieceType type, int row, int col, bool isWhite)
    {
        // Mirror row for black pieces
        var tableRow = isWhite ? row : 7 - row;

        return type switch
        {
            ChessPieceType.Pawn => PawnTable[tableRow, col],
            ChessPieceType.Knight => KnightTable[tableRow, col],
            ChessPieceType.Bishop => BishopTable[tableRow, col],
            ChessPieceType.Rook => RookTable[tableRow, col],
            ChessPieceType.Queen => QueenTable[tableRow, col],
            ChessPieceType.King => KingMiddleGameTable[tableRow, col],
            _ => 0
        };
    }

    private int CountMobility(IReadOnlyList<ChessSquareViewModel> squares, bool isWhite)
    {
        int mobility = 0;

        foreach (var square in squares)
        {
            if (square.Piece == null || square.Piece.IsWhite != isWhite) continue;

            // Count approximate mobility (simplified)
            mobility += square.Piece.Type switch
            {
                ChessPieceType.Knight => 4,
                ChessPieceType.Bishop => 4,
                ChessPieceType.Rook => 4,
                ChessPieceType.Queen => 8,
                _ => 1
            };
        }

        return mobility;
    }
}
