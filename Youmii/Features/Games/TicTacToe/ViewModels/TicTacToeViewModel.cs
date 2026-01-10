using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Youmii.Features.Games.TicTacToe.Models;
using Youmii.ViewModels;

namespace Youmii.Features.Games.TicTacToe.ViewModels;

/// <summary>
/// ViewModel for Tic Tac Toe game.
/// </summary>
public sealed class TicTacToeViewModel : ViewModelBase
{
    private readonly Random _random = new();
    private readonly DispatcherTimer _aiTimer;
    private TicTacToePlayer _currentPlayer = TicTacToePlayer.X;
    private TicTacToeGameState _gameState = TicTacToeGameState.Playing;
    private bool _isVsAi = true;
    private int _playerScore;
    private int _aiScore;
    private int _draws;

    public TicTacToeViewModel()
    {
        Cells = [];
        
        CellClickedCommand = new RelayCommand<TicTacToeCellViewModel>(OnCellClicked);
        NewGameCommand = new RelayCommand(NewGame);
        ToggleAiCommand = new RelayCommand(ToggleAi);

        _aiTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
        _aiTimer.Tick += OnAiTimerTick;

        InitializeBoard();
    }

    public ObservableCollection<TicTacToeCellViewModel> Cells { get; }

    public TicTacToePlayer CurrentPlayer
    {
        get => _currentPlayer;
        private set
        {
            if (SetProperty(ref _currentPlayer, value))
            {
                OnPropertyChanged(nameof(CurrentPlayerDisplay));
                OnPropertyChanged(nameof(StatusText));
            }
        }
    }

    public TicTacToeGameState GameState
    {
        get => _gameState;
        private set
        {
            if (SetProperty(ref _gameState, value))
            {
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(IsGameOver));
            }
        }
    }

    public bool IsVsAi
    {
        get => _isVsAi;
        set
        {
            if (SetProperty(ref _isVsAi, value))
            {
                OnPropertyChanged(nameof(ModeDisplay));
                NewGame();
            }
        }
    }

    public string CurrentPlayerDisplay => CurrentPlayer == TicTacToePlayer.X ? "X" : "O";
    public string ModeDisplay => IsVsAi ? "vs AI" : "2 Players";
    public bool IsGameOver => GameState != TicTacToeGameState.Playing;

    public string StatusText => GameState switch
    {
        TicTacToeGameState.XWins => "X Wins!",
        TicTacToeGameState.OWins => "O Wins!",
        TicTacToeGameState.Draw => "It's a Draw!",
        _ => $"{CurrentPlayerDisplay}'s Turn"
    };

    public int PlayerScore
    {
        get => _playerScore;
        private set => SetProperty(ref _playerScore, value);
    }

    public int AiScore
    {
        get => _aiScore;
        private set => SetProperty(ref _aiScore, value);
    }

    public int Draws
    {
        get => _draws;
        private set => SetProperty(ref _draws, value);
    }

    public ICommand CellClickedCommand { get; }
    public ICommand NewGameCommand { get; }
    public ICommand ToggleAiCommand { get; }

    public event EventHandler? RequestClose;

    private void InitializeBoard()
    {
        Cells.Clear();
        CurrentPlayer = TicTacToePlayer.X;
        GameState = TicTacToeGameState.Playing;

        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                Cells.Add(new TicTacToeCellViewModel
                {
                    Row = row,
                    Column = col
                });
            }
        }
    }

    private void OnCellClicked(TicTacToeCellViewModel? cell)
    {
        if (cell == null || cell.Player != TicTacToePlayer.None || IsGameOver) return;
        if (IsVsAi && CurrentPlayer == TicTacToePlayer.O) return; // AI's turn

        MakeMove(cell);

        if (!IsGameOver && IsVsAi)
        {
            _aiTimer.Start();
        }
    }

    private void MakeMove(TicTacToeCellViewModel cell)
    {
        cell.Player = CurrentPlayer;
        
        if (CheckWin(CurrentPlayer))
        {
            GameState = CurrentPlayer == TicTacToePlayer.X 
                ? TicTacToeGameState.XWins 
                : TicTacToeGameState.OWins;
            
            UpdateScore();
            return;
        }

        if (Cells.All(c => c.Player != TicTacToePlayer.None))
        {
            GameState = TicTacToeGameState.Draw;
            Draws++;
            return;
        }

        CurrentPlayer = CurrentPlayer == TicTacToePlayer.X 
            ? TicTacToePlayer.O 
            : TicTacToePlayer.X;
    }

    private void OnAiTimerTick(object? sender, EventArgs e)
    {
        _aiTimer.Stop();
        MakeAiMove();
    }

    private void MakeAiMove()
    {
        if (IsGameOver || CurrentPlayer != TicTacToePlayer.O) return;

        var bestMove = FindBestMove();
        if (bestMove != null)
        {
            MakeMove(bestMove);
        }
    }

    private TicTacToeCellViewModel? FindBestMove()
    {
        var emptyCells = Cells.Where(c => c.Player == TicTacToePlayer.None).ToList();
        if (emptyCells.Count == 0) return null;

        // Try to win
        foreach (var cell in emptyCells)
        {
            cell.Player = TicTacToePlayer.O;
            if (CheckWin(TicTacToePlayer.O))
            {
                cell.Player = TicTacToePlayer.None;
                return cell;
            }
            cell.Player = TicTacToePlayer.None;
        }

        // Block opponent
        foreach (var cell in emptyCells)
        {
            cell.Player = TicTacToePlayer.X;
            if (CheckWin(TicTacToePlayer.X))
            {
                cell.Player = TicTacToePlayer.None;
                return cell;
            }
            cell.Player = TicTacToePlayer.None;
        }

        // Take center
        var center = GetCell(1, 1);
        if (center?.Player == TicTacToePlayer.None)
            return center;

        // Take corners
        var corners = new[] { GetCell(0, 0), GetCell(0, 2), GetCell(2, 0), GetCell(2, 2) }
            .Where(c => c?.Player == TicTacToePlayer.None)
            .ToList();
        if (corners.Count > 0)
            return corners[_random.Next(corners.Count)];

        // Random empty cell
        return emptyCells[_random.Next(emptyCells.Count)];
    }

    private bool CheckWin(TicTacToePlayer player)
    {
        // Rows
        for (int row = 0; row < 3; row++)
        {
            if (GetCell(row, 0)?.Player == player &&
                GetCell(row, 1)?.Player == player &&
                GetCell(row, 2)?.Player == player)
            {
                MarkWinningCells(GetCell(row, 0), GetCell(row, 1), GetCell(row, 2));
                return true;
            }
        }

        // Columns
        for (int col = 0; col < 3; col++)
        {
            if (GetCell(0, col)?.Player == player &&
                GetCell(1, col)?.Player == player &&
                GetCell(2, col)?.Player == player)
            {
                MarkWinningCells(GetCell(0, col), GetCell(1, col), GetCell(2, col));
                return true;
            }
        }

        // Diagonals
        if (GetCell(0, 0)?.Player == player &&
            GetCell(1, 1)?.Player == player &&
            GetCell(2, 2)?.Player == player)
        {
            MarkWinningCells(GetCell(0, 0), GetCell(1, 1), GetCell(2, 2));
            return true;
        }

        if (GetCell(0, 2)?.Player == player &&
            GetCell(1, 1)?.Player == player &&
            GetCell(2, 0)?.Player == player)
        {
            MarkWinningCells(GetCell(0, 2), GetCell(1, 1), GetCell(2, 0));
            return true;
        }

        return false;
    }

    private void MarkWinningCells(params TicTacToeCellViewModel?[] cells)
    {
        foreach (var cell in cells)
        {
            if (cell != null)
                cell.IsWinningCell = true;
        }
    }

    private TicTacToeCellViewModel? GetCell(int row, int col)
    {
        return Cells.FirstOrDefault(c => c.Row == row && c.Column == col);
    }

    private void UpdateScore()
    {
        if (GameState == TicTacToeGameState.XWins)
            PlayerScore++;
        else if (GameState == TicTacToeGameState.OWins)
            AiScore++;
    }

    private void NewGame()
    {
        _aiTimer.Stop();
        InitializeBoard();
    }

    private void ToggleAi()
    {
        IsVsAi = !IsVsAi;
    }
}

/// <summary>
/// ViewModel for individual cells.
/// </summary>
public sealed class TicTacToeCellViewModel : ViewModelBase
{
    private TicTacToePlayer _player = TicTacToePlayer.None;
    private bool _isWinningCell;

    public int Row { get; init; }
    public int Column { get; init; }

    public TicTacToePlayer Player
    {
        get => _player;
        set
        {
            if (SetProperty(ref _player, value))
            {
                OnPropertyChanged(nameof(Symbol));
                OnPropertyChanged(nameof(SymbolColor));
            }
        }
    }

    public bool IsWinningCell
    {
        get => _isWinningCell;
        set => SetProperty(ref _isWinningCell, value);
    }

    public string Symbol => Player switch
    {
        TicTacToePlayer.X => "X",
        TicTacToePlayer.O => "O",
        _ => string.Empty
    };

    public Brush SymbolColor => Player switch
    {
        TicTacToePlayer.X => new SolidColorBrush(Color.FromRgb(233, 30, 99)),  // Pink
        TicTacToePlayer.O => new SolidColorBrush(Color.FromRgb(33, 150, 243)), // Blue
        _ => Brushes.Transparent
    };
}
