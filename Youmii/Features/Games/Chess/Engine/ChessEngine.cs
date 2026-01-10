using Youmii.Features.Games.Chess.Models;
using Youmii.Features.Games.Chess.ViewModels;

namespace Youmii.Features.Games.Chess.Engine;

/// <summary>
/// Difficulty levels for the chess AI.
/// </summary>
public enum ChessDifficulty
{
    Easy = 1,
    Medium = 2,
    Hard = 3
}

/// <summary>
/// Chess AI engine that calculates moves based on difficulty level.
/// </summary>
public sealed class ChessEngine
{
    private readonly Random _random = new();
    private readonly MoveEvaluator _evaluator = new();

    /// <summary>
    /// Gets the best move for the AI based on the current board state and difficulty.
    /// </summary>
    /// <param name="squares">The current board squares.</param>
    /// <param name="isWhite">Whether the AI is playing as white.</param>
    /// <param name="difficulty">The difficulty level.</param>
    /// <returns>The best move as (fromSquare, toSquare), or null if no moves available.</returns>
    public (ChessSquareViewModel from, ChessSquareViewModel to)? GetBestMove(
        IReadOnlyList<ChessSquareViewModel> squares,
        bool isWhite,
        ChessDifficulty difficulty)
    {
        var allMoves = GetAllValidMoves(squares, isWhite);
        if (allMoves.Count == 0) return null;

        return difficulty switch
        {
            ChessDifficulty.Easy => GetEasyMove(allMoves, squares),
            ChessDifficulty.Medium => GetMediumMove(allMoves, squares, isWhite),
            ChessDifficulty.Hard => GetHardMove(allMoves, squares, isWhite),
            _ => GetEasyMove(allMoves, squares)
        };
    }

    /// <summary>
    /// Easy: Makes random moves with slight preference for captures.
    /// </summary>
    private (ChessSquareViewModel from, ChessSquareViewModel to) GetEasyMove(
        List<(ChessSquareViewModel from, ChessSquareViewModel to)> moves,
        IReadOnlyList<ChessSquareViewModel> squares)
    {
        // 30% chance to make a capture if available
        var captures = moves.Where(m => m.to.Piece != null).ToList();
        if (captures.Count > 0 && _random.NextDouble() < 0.3)
        {
            return captures[_random.Next(captures.Count)];
        }

        // Otherwise random move
        return moves[_random.Next(moves.Count)];
    }

    /// <summary>
    /// Medium: Uses basic evaluation with 2-ply depth, occasional mistakes.
    /// </summary>
    private (ChessSquareViewModel from, ChessSquareViewModel to) GetMediumMove(
        List<(ChessSquareViewModel from, ChessSquareViewModel to)> moves,
        IReadOnlyList<ChessSquareViewModel> squares,
        bool isWhite)
    {
        // 15% chance to make a suboptimal move
        if (_random.NextDouble() < 0.15)
        {
            return GetEasyMove(moves, squares);
        }

        return GetBestMoveByEvaluation(moves, squares, isWhite, depth: 2);
    }

    /// <summary>
    /// Hard: Uses deeper search with full evaluation.
    /// </summary>
    private (ChessSquareViewModel from, ChessSquareViewModel to) GetHardMove(
        List<(ChessSquareViewModel from, ChessSquareViewModel to)> moves,
        IReadOnlyList<ChessSquareViewModel> squares,
        bool isWhite)
    {
        return GetBestMoveByEvaluation(moves, squares, isWhite, depth: 3);
    }

    private (ChessSquareViewModel from, ChessSquareViewModel to) GetBestMoveByEvaluation(
        List<(ChessSquareViewModel from, ChessSquareViewModel to)> moves,
        IReadOnlyList<ChessSquareViewModel> squares,
        bool isWhite,
        int depth)
    {
        var bestMove = moves[0];
        var bestScore = int.MinValue;

        foreach (var move in moves)
        {
            // Simulate the move
            var capturedPiece = move.to.Piece;
            var movedPiece = move.from.Piece;
            move.to.Piece = movedPiece;
            move.from.Piece = null;

            // Evaluate position
            var score = -Minimax(squares, depth - 1, !isWhite, int.MinValue, int.MaxValue);

            // Add randomness to avoid predictable play
            score += _random.Next(-10, 10);

            // Undo move
            move.from.Piece = movedPiece;
            move.to.Piece = capturedPiece;

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    private int Minimax(IReadOnlyList<ChessSquareViewModel> squares, int depth, bool isMaximizing, int alpha, int beta)
    {
        if (depth == 0)
        {
            return _evaluator.EvaluatePosition(squares, isMaximizing);
        }

        var moves = GetAllValidMoves(squares, isMaximizing);
        if (moves.Count == 0)
        {
            // No moves - either checkmate or stalemate
            return isMaximizing ? -10000 : 10000;
        }

        if (isMaximizing)
        {
            var maxEval = int.MinValue;
            foreach (var move in moves)
            {
                var capturedPiece = move.to.Piece;
                var movedPiece = move.from.Piece;
                move.to.Piece = movedPiece;
                move.from.Piece = null;

                var eval = Minimax(squares, depth - 1, false, alpha, beta);

                move.from.Piece = movedPiece;
                move.to.Piece = capturedPiece;

                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);
                if (beta <= alpha) break;
            }
            return maxEval;
        }
        else
        {
            var minEval = int.MaxValue;
            foreach (var move in moves)
            {
                var capturedPiece = move.to.Piece;
                var movedPiece = move.from.Piece;
                move.to.Piece = movedPiece;
                move.from.Piece = null;

                var eval = Minimax(squares, depth - 1, true, alpha, beta);

                move.from.Piece = movedPiece;
                move.to.Piece = capturedPiece;

                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval);
                if (beta <= alpha) break;
            }
            return minEval;
        }
    }

    private List<(ChessSquareViewModel from, ChessSquareViewModel to)> GetAllValidMoves(
        IReadOnlyList<ChessSquareViewModel> squares,
        bool isWhite)
    {
        var moves = new List<(ChessSquareViewModel from, ChessSquareViewModel to)>();

        foreach (var from in squares)
        {
            if (from.Piece == null || from.Piece.IsWhite != isWhite) continue;

            foreach (var to in squares)
            {
                if (IsValidMove(from, to, squares))
                {
                    moves.Add((from, to));
                }
            }
        }

        return moves;
    }

    private bool IsValidMove(ChessSquareViewModel from, ChessSquareViewModel to, IReadOnlyList<ChessSquareViewModel> squares)
    {
        if (from.Piece == null) return false;
        if (to.Piece != null && to.Piece.IsWhite == from.Piece.IsWhite) return false;
        if (from == to) return false;

        int rowDiff = to.Row - from.Row;
        int colDiff = to.Column - from.Column;
        int absRowDiff = Math.Abs(rowDiff);
        int absColDiff = Math.Abs(colDiff);

        return from.Piece.Type switch
        {
            ChessPieceType.Pawn => IsValidPawnMove(from, to, rowDiff, colDiff, absColDiff, squares),
            ChessPieceType.Rook => (rowDiff == 0 || colDiff == 0) && IsPathClear(from, to, squares),
            ChessPieceType.Knight => (absRowDiff == 2 && absColDiff == 1) || (absRowDiff == 1 && absColDiff == 2),
            ChessPieceType.Bishop => absRowDiff == absColDiff && IsPathClear(from, to, squares),
            ChessPieceType.Queen => (rowDiff == 0 || colDiff == 0 || absRowDiff == absColDiff) && IsPathClear(from, to, squares),
            ChessPieceType.King => absRowDiff <= 1 && absColDiff <= 1,
            _ => false
        };
    }

    private bool IsValidPawnMove(ChessSquareViewModel from, ChessSquareViewModel to, int rowDiff, int colDiff, int absColDiff, IReadOnlyList<ChessSquareViewModel> squares)
    {
        if (from.Piece == null) return false;

        int direction = from.Piece.IsWhite ? -1 : 1;
        int startRow = from.Piece.IsWhite ? 6 : 1;

        // Forward move
        if (colDiff == 0 && to.Piece == null)
        {
            if (rowDiff == direction) return true;
            if (from.Row == startRow && rowDiff == 2 * direction)
            {
                var middleSquare = GetSquare(from.Row + direction, from.Column, squares);
                return middleSquare?.Piece == null;
            }
        }

        // Capture
        if (absColDiff == 1 && rowDiff == direction && to.Piece != null)
        {
            return true;
        }

        return false;
    }

    private bool IsPathClear(ChessSquareViewModel from, ChessSquareViewModel to, IReadOnlyList<ChessSquareViewModel> squares)
    {
        int rowStep = Math.Sign(to.Row - from.Row);
        int colStep = Math.Sign(to.Column - from.Column);

        int currentRow = from.Row + rowStep;
        int currentCol = from.Column + colStep;

        while (currentRow != to.Row || currentCol != to.Column)
        {
            var square = GetSquare(currentRow, currentCol, squares);
            if (square?.Piece != null) return false;

            currentRow += rowStep;
            currentCol += colStep;
        }

        return true;
    }

    private static ChessSquareViewModel? GetSquare(int row, int col, IReadOnlyList<ChessSquareViewModel> squares)
    {
        if (row < 0 || row > 7 || col < 0 || col > 7) return null;
        return squares[row * 8 + col];
    }
}
