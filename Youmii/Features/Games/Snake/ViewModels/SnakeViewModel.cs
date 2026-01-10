using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Youmii.Features.Games.Snake.Models;
using Youmii.ViewModels;

namespace Youmii.Features.Games.Snake.ViewModels;

/// <summary>
/// ViewModel for Snake game.
/// </summary>
public sealed class SnakeViewModel : ViewModelBase
{
    private const int GridSize = 15;
    private const int InitialSnakeLength = 3;
    private const int BaseSpeed = 150; // ms between moves

    private readonly Random _random = new();
    private readonly DispatcherTimer _gameTimer;
    private readonly LinkedList<Position> _snake = new();
    private Position _food;
    private SnakeDirection _direction = SnakeDirection.Right;
    private SnakeDirection _nextDirection = SnakeDirection.Right;
    private SnakeGameState _gameState = SnakeGameState.Ready;
    private int _score;
    private int _highScore;

    public SnakeViewModel()
    {
        _gameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(BaseSpeed) };
        _gameTimer.Tick += OnGameTick;

        StartGameCommand = new RelayCommand(StartGame);
        MoveUpCommand = new RelayCommand(() => ChangeDirection(SnakeDirection.Up));
        MoveDownCommand = new RelayCommand(() => ChangeDirection(SnakeDirection.Down));
        MoveLeftCommand = new RelayCommand(() => ChangeDirection(SnakeDirection.Left));
        MoveRightCommand = new RelayCommand(() => ChangeDirection(SnakeDirection.Right));

        InitializeGame();
    }

    public int GridWidth => GridSize;
    public int GridHeight => GridSize;
    public int CellSize => 20;
    public int BoardWidth => GridSize * CellSize;
    public int BoardHeight => GridSize * CellSize;

    public SnakeGameState GameState
    {
        get => _gameState;
        private set
        {
            if (SetProperty(ref _gameState, value))
            {
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(IsGameOver));
                OnPropertyChanged(nameof(ShowStartMessage));
            }
        }
    }

    public int Score
    {
        get => _score;
        private set => SetProperty(ref _score, value);
    }

    public int HighScore
    {
        get => _highScore;
        private set => SetProperty(ref _highScore, value);
    }

    public string StatusText => GameState switch
    {
        SnakeGameState.Ready => "Press Space to Start",
        SnakeGameState.GameOver => $"Game Over! Score: {Score}",
        _ => string.Empty
    };

    public bool IsGameOver => GameState == SnakeGameState.GameOver;
    public bool ShowStartMessage => GameState == SnakeGameState.Ready;

    // Visual representation for binding
    public Brush[,] Grid { get; private set; } = new Brush[GridSize, GridSize];

    public ICommand StartGameCommand { get; }
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }
    public ICommand MoveLeftCommand { get; }
    public ICommand MoveRightCommand { get; }

    // Event to notify view to redraw
    public event EventHandler? BoardChanged;
    public event EventHandler? RequestClose;

    public IEnumerable<Position> GetSnakePositions() => _snake;
    public Position GetFoodPosition() => _food;

    private void InitializeGame()
    {
        _snake.Clear();
        _direction = SnakeDirection.Right;
        _nextDirection = SnakeDirection.Right;
        Score = 0;

        // Place snake in the center
        int startX = GridSize / 2;
        int startY = GridSize / 2;

        for (int i = 0; i < InitialSnakeLength; i++)
        {
            _snake.AddLast(new Position(startX - i, startY));
        }

        SpawnFood();
        UpdateBoard();
    }

    private void StartGame()
    {
        if (GameState == SnakeGameState.Playing) return;

        InitializeGame();
        GameState = SnakeGameState.Playing;
        _gameTimer.Interval = TimeSpan.FromMilliseconds(BaseSpeed);
        _gameTimer.Start();
    }

    private void ChangeDirection(SnakeDirection newDirection)
    {
        if (GameState != SnakeGameState.Playing)
        {
            if (GameState == SnakeGameState.Ready || GameState == SnakeGameState.GameOver)
            {
                StartGame();
            }
            return;
        }

        // Prevent 180-degree turns
        var opposites = new Dictionary<SnakeDirection, SnakeDirection>
        {
            { SnakeDirection.Up, SnakeDirection.Down },
            { SnakeDirection.Down, SnakeDirection.Up },
            { SnakeDirection.Left, SnakeDirection.Right },
            { SnakeDirection.Right, SnakeDirection.Left }
        };

        if (opposites[newDirection] != _direction)
        {
            _nextDirection = newDirection;
        }
    }

    private void OnGameTick(object? sender, EventArgs e)
    {
        _direction = _nextDirection;
        MoveSnake();
    }

    private void MoveSnake()
    {
        var head = _snake.First!.Value;
        Position newHead = _direction switch
        {
            SnakeDirection.Up => new Position(head.X, head.Y - 1),
            SnakeDirection.Down => new Position(head.X, head.Y + 1),
            SnakeDirection.Left => new Position(head.X - 1, head.Y),
            SnakeDirection.Right => new Position(head.X + 1, head.Y),
            _ => head
        };

        // Check wall collision
        if (newHead.X < 0 || newHead.X >= GridSize || 
            newHead.Y < 0 || newHead.Y >= GridSize)
        {
            EndGame();
            return;
        }

        // Check self collision
        if (_snake.Any(p => p == newHead))
        {
            EndGame();
            return;
        }

        _snake.AddFirst(newHead);

        // Check food collision
        if (newHead == _food)
        {
            Score += 10;
            SpawnFood();
            
            // Speed up slightly
            var newInterval = Math.Max(50, BaseSpeed - (Score / 50) * 10);
            _gameTimer.Interval = TimeSpan.FromMilliseconds(newInterval);
        }
        else
        {
            _snake.RemoveLast();
        }

        UpdateBoard();
    }

    private void SpawnFood()
    {
        var emptyPositions = new List<Position>();
        
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                var pos = new Position(x, y);
                if (!_snake.Contains(pos))
                {
                    emptyPositions.Add(pos);
                }
            }
        }

        if (emptyPositions.Count > 0)
        {
            _food = emptyPositions[_random.Next(emptyPositions.Count)];
        }
    }

    private void UpdateBoard()
    {
        BoardChanged?.Invoke(this, EventArgs.Empty);
    }

    private void EndGame()
    {
        _gameTimer.Stop();
        GameState = SnakeGameState.GameOver;
        
        if (Score > HighScore)
        {
            HighScore = Score;
        }
    }

    public void HandleKeyDown(Key key)
    {
        switch (key)
        {
            case Key.Up:
            case Key.W:
                ChangeDirection(SnakeDirection.Up);
                break;
            case Key.Down:
            case Key.S:
                ChangeDirection(SnakeDirection.Down);
                break;
            case Key.Left:
            case Key.A:
                ChangeDirection(SnakeDirection.Left);
                break;
            case Key.Right:
            case Key.D:
                ChangeDirection(SnakeDirection.Right);
                break;
            case Key.Space:
                if (GameState != SnakeGameState.Playing)
                    StartGame();
                break;
        }
    }
}
