namespace Youmii.Features.Games.Snake.Models;

/// <summary>
/// Direction the snake is moving.
/// </summary>
public enum SnakeDirection
{
    Up,
    Down,
    Left,
    Right
}

/// <summary>
/// Represents a position on the game board.
/// </summary>
public readonly struct Position(int x, int y)
{
    public int X { get; } = x;
    public int Y { get; } = y;

    public static bool operator ==(Position a, Position b) => a.X == b.X && a.Y == b.Y;
    public static bool operator !=(Position a, Position b) => !(a == b);
    
    public override bool Equals(object? obj) => obj is Position p && this == p;
    public override int GetHashCode() => HashCode.Combine(X, Y);
}

/// <summary>
/// Game state.
/// </summary>
public enum SnakeGameState
{
    Ready,
    Playing,
    GameOver
}
