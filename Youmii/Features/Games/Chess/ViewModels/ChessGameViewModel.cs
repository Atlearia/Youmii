using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Youmii.Features.Games.Chess.Engine;
using Youmii.Features.Games.Chess.Models;
using Youmii.ViewModels;

namespace Youmii.Features.Games.Chess.ViewModels;

/// <summary>
/// ViewModel for the Chess game.
/// </summary>
public sealed class ChessGameViewModel : ViewModelBase
{
    private ChessSquareViewModel? _selectedSquare;
    private bool _isWhiteTurn = true;
    private string _gameStatus = string.Empty;
    private readonly List<ChessMove> _moveHistory = [];
    
    // AI Engine
    private readonly ChessEngine _engine = new();
    private bool _isPlayingAgainstAi = true;
    private bool _isAiWhite = false; // AI plays black by default
    private ChessDifficulty _difficulty = ChessDifficulty.Medium;
    private bool _isAiThinking;
    private readonly DispatcherTimer _aiMoveTimer;

    public ChessGameViewModel()
    {
        Squares = [];
        RowLabels = ["8", "7", "6", "5", "4", "3", "2", "1"];
        ColumnLabels = ["a", "b", "c", "d", "e", "f", "g", "h"];

        SquareClickedCommand = new RelayCommand<ChessSquareViewModel>(OnSquareClicked);
        NewGameCommand = new RelayCommand(NewGame);
        UndoMoveCommand = new RelayCommand(UndoMove, CanUndoMove);
        HintCommand = new RelayCommand(ShowHint);
        SetDifficultyCommand = new RelayCommand<string>(SetDifficulty);
        ToggleAiCommand = new RelayCommand(ToggleAi);

        // Timer for AI move delay (makes it feel more natural)
        _aiMoveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _aiMoveTimer.Tick += OnAiMoveTimerTick;

        InitializeBoard();
    }

    #region Properties

    public ObservableCollection<ChessSquareViewModel> Squares { get; }
    public ObservableCollection<string> RowLabels { get; }
    public ObservableCollection<string> ColumnLabels { get; }

    public bool IsWhiteTurn
    {
        get => _isWhiteTurn;
        private set
        {
            if (SetProperty(ref _isWhiteTurn, value))
            {
                OnPropertyChanged(nameof(CurrentTurnDisplay));
            }
        }
    }

    public string CurrentTurnDisplay => IsWhiteTurn ? "White" : "Black";

    public string GameStatus
    {
        get => _gameStatus;
        private set => SetProperty(ref _gameStatus, value);
    }

    public string WhiteCapturedPieces { get; private set; } = string.Empty;
    public string BlackCapturedPieces { get; private set; } = string.Empty;

    public bool IsPlayingAgainstAi
    {
        get => _isPlayingAgainstAi;
        set
        {
            if (SetProperty(ref _isPlayingAgainstAi, value))
            {
                OnPropertyChanged(nameof(AiModeDisplay));
            }
        }
    }

    public ChessDifficulty Difficulty
    {
        get => _difficulty;
        set
        {
            if (SetProperty(ref _difficulty, value))
            {
                OnPropertyChanged(nameof(DifficultyDisplay));
                OnPropertyChanged(nameof(IsEasySelected));
                OnPropertyChanged(nameof(IsMediumSelected));
                OnPropertyChanged(nameof(IsHardSelected));
            }
        }
    }

    public string DifficultyDisplay => Difficulty.ToString();
    public string AiModeDisplay => IsPlayingAgainstAi ? "vs AI" : "2 Players";

    public bool IsEasySelected => Difficulty == ChessDifficulty.Easy;
    public bool IsMediumSelected => Difficulty == ChessDifficulty.Medium;
    public bool IsHardSelected => Difficulty == ChessDifficulty.Hard;

    public bool IsAiThinking
    {
        get => _isAiThinking;
        private set => SetProperty(ref _isAiThinking, value);
    }

    #endregion

    #region Commands

    public ICommand SquareClickedCommand { get; }
    public ICommand NewGameCommand { get; }
    public ICommand UndoMoveCommand { get; }
    public ICommand HintCommand { get; }
    public ICommand SetDifficultyCommand { get; }
    public ICommand ToggleAiCommand { get; }

    #endregion

    #region Events

    public event EventHandler? RequestClose;

    #endregion

    #region Private Methods

    private void InitializeBoard()
    {
        Squares.Clear();
        _moveHistory.Clear();
        IsWhiteTurn = true;
        GameStatus = string.Empty;
        WhiteCapturedPieces = string.Empty;
        BlackCapturedPieces = string.Empty;
        IsAiThinking = false;
        OnPropertyChanged(nameof(WhiteCapturedPieces));
        OnPropertyChanged(nameof(BlackCapturedPieces));

        var lightBrush = new SolidColorBrush(Color.FromRgb(252, 228, 236)); // #FFFCE4EC
        var darkBrush = new SolidColorBrush(Color.FromRgb(248, 187, 217));  // #FFF8BBD9

        // Create 64 squares
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                var isLight = (row + col) % 2 == 0;
                var square = new ChessSquareViewModel
                {
                    Row = row,
                    Column = col,
                    Background = isLight ? lightBrush : darkBrush,
                    OriginalBackground = isLight ? lightBrush : darkBrush
                };
                Squares.Add(square);
            }
        }

        // Place pieces
        SetupInitialPieces();
    }

    private void SetupInitialPieces()
    {
        // Black pieces (top, rows 0-1)
        SetPiece(0, 0, ChessPieceType.Rook, false);
        SetPiece(0, 1, ChessPieceType.Knight, false);
        SetPiece(0, 2, ChessPieceType.Bishop, false);
        SetPiece(0, 3, ChessPieceType.Queen, false);
        SetPiece(0, 4, ChessPieceType.King, false);
        SetPiece(0, 5, ChessPieceType.Bishop, false);
        SetPiece(0, 6, ChessPieceType.Knight, false);
        SetPiece(0, 7, ChessPieceType.Rook, false);

        for (int col = 0; col < 8; col++)
        {
            SetPiece(1, col, ChessPieceType.Pawn, false);
        }

        // White pieces (bottom, rows 6-7)
        for (int col = 0; col < 8; col++)
        {
            SetPiece(6, col, ChessPieceType.Pawn, true);
        }

        SetPiece(7, 0, ChessPieceType.Rook, true);
        SetPiece(7, 1, ChessPieceType.Knight, true);
        SetPiece(7, 2, ChessPieceType.Bishop, true);
        SetPiece(7, 3, ChessPieceType.Queen, true);
        SetPiece(7, 4, ChessPieceType.King, true);
        SetPiece(7, 5, ChessPieceType.Bishop, true);
        SetPiece(7, 6, ChessPieceType.Knight, true);
        SetPiece(7, 7, ChessPieceType.Rook, true);
    }

    private void SetPiece(int row, int col, ChessPieceType type, bool isWhite)
    {
        var square = GetSquare(row, col);
        if (square != null)
        {
            square.Piece = new ChessPiece { Type = type, IsWhite = isWhite };
        }
    }

    private ChessSquareViewModel? GetSquare(int row, int col)
    {
        if (row < 0 || row > 7 || col < 0 || col > 7) return null;
        return Squares[row * 8 + col];
    }

    private void OnSquareClicked(ChessSquareViewModel? square)
    {
        if (square == null) return;
        
        // Don't allow moves while AI is thinking or game is over
        if (IsAiThinking || !string.IsNullOrEmpty(GameStatus)) return;
        
        // In AI mode, don't allow moving AI's pieces
        if (IsPlayingAgainstAi && IsWhiteTurn == _isAiWhite) return;

        // If no piece selected, try to select one
        if (_selectedSquare == null)
        {
            if (square.Piece != null && square.Piece.IsWhite == IsWhiteTurn)
            {
                SelectSquare(square);
            }
            return;
        }

        // If clicking on same square, deselect
        if (_selectedSquare == square)
        {
            DeselectAll();
            return;
        }

        // If clicking on own piece, select that piece instead
        if (square.Piece != null && square.Piece.IsWhite == IsWhiteTurn)
        {
            DeselectAll();
            SelectSquare(square);
            return;
        }

        // Try to make a move
        if (IsValidMove(_selectedSquare, square))
        {
            MakeMove(_selectedSquare, square);
            
            // Trigger AI move if playing against AI
            if (IsPlayingAgainstAi && string.IsNullOrEmpty(GameStatus))
            {
                TriggerAiMove();
            }
        }

        DeselectAll();
    }

    private void TriggerAiMove()
    {
        IsAiThinking = true;
        GameStatus = "AI thinking...";
        _aiMoveTimer.Start();
    }

    private void OnAiMoveTimerTick(object? sender, EventArgs e)
    {
        _aiMoveTimer.Stop();
        MakeAiMove();
    }

    private void MakeAiMove()
    {
        if (!IsPlayingAgainstAi || IsWhiteTurn != _isAiWhite)
        {
            IsAiThinking = false;
            GameStatus = string.Empty;
            return;
        }

        var bestMove = _engine.GetBestMove(Squares, _isAiWhite, Difficulty);
        
        if (bestMove.HasValue)
        {
            MakeMove(bestMove.Value.from, bestMove.Value.to);
        }

        IsAiThinking = false;
        if (string.IsNullOrEmpty(GameStatus) || GameStatus == "AI thinking...")
        {
            GameStatus = string.Empty;
        }
    }

    private void SelectSquare(ChessSquareViewModel square)
    {
        _selectedSquare = square;
        square.IsSelected = true;

        // Show valid moves
        ShowValidMoves(square);
    }

    private void DeselectAll()
    {
        _selectedSquare = null;
        foreach (var sq in Squares)
        {
            sq.IsSelected = false;
            sq.IsValidMove = false;
            sq.Background = sq.OriginalBackground;
        }
    }

    private void ShowValidMoves(ChessSquareViewModel fromSquare)
    {
        if (fromSquare.Piece == null) return;

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                var toSquare = GetSquare(row, col);
                if (toSquare != null && IsValidMove(fromSquare, toSquare))
                {
                    toSquare.IsValidMove = true;
                }
            }
        }
    }

    private bool IsValidMove(ChessSquareViewModel from, ChessSquareViewModel to)
    {
        if (from.Piece == null) return false;
        if (to.Piece != null && to.Piece.IsWhite == from.Piece.IsWhite) return false;

        int rowDiff = to.Row - from.Row;
        int colDiff = to.Column - from.Column;
        int absRowDiff = Math.Abs(rowDiff);
        int absColDiff = Math.Abs(colDiff);

        return from.Piece.Type switch
        {
            ChessPieceType.Pawn => IsValidPawnMove(from, to, rowDiff, colDiff, absColDiff),
            ChessPieceType.Rook => (rowDiff == 0 || colDiff == 0) && IsPathClear(from, to),
            ChessPieceType.Knight => (absRowDiff == 2 && absColDiff == 1) || (absRowDiff == 1 && absColDiff == 2),
            ChessPieceType.Bishop => absRowDiff == absColDiff && IsPathClear(from, to),
            ChessPieceType.Queen => (rowDiff == 0 || colDiff == 0 || absRowDiff == absColDiff) && IsPathClear(from, to),
            ChessPieceType.King => absRowDiff <= 1 && absColDiff <= 1,
            _ => false
        };
    }

    private bool IsValidPawnMove(ChessSquareViewModel from, ChessSquareViewModel to, int rowDiff, int colDiff, int absColDiff)
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
                var middleSquare = GetSquare(from.Row + direction, from.Column);
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

    private bool IsPathClear(ChessSquareViewModel from, ChessSquareViewModel to)
    {
        int rowStep = Math.Sign(to.Row - from.Row);
        int colStep = Math.Sign(to.Column - from.Column);

        int currentRow = from.Row + rowStep;
        int currentCol = from.Column + colStep;

        while (currentRow != to.Row || currentCol != to.Column)
        {
            var square = GetSquare(currentRow, currentCol);
            if (square?.Piece != null) return false;

            currentRow += rowStep;
            currentCol += colStep;
        }

        return true;
    }

    private void MakeMove(ChessSquareViewModel from, ChessSquareViewModel to)
    {
        if (from.Piece == null) return;

        // Record move for undo
        _moveHistory.Add(new ChessMove
        {
            FromRow = from.Row,
            FromCol = from.Column,
            ToRow = to.Row,
            ToCol = to.Column,
            MovedPiece = from.Piece,
            CapturedPiece = to.Piece
        });

        // Handle capture
        if (to.Piece != null)
        {
            AddCapturedPiece(to.Piece);

            // Check for king capture (game over)
            if (to.Piece.Type == ChessPieceType.King)
            {
                GameStatus = IsWhiteTurn ? "White Wins!" : "Black Wins!";
            }
        }

        // Move piece
        to.Piece = from.Piece;
        from.Piece = null;

        // Check for pawn promotion
        if (to.Piece.Type == ChessPieceType.Pawn)
        {
            if ((to.Piece.IsWhite && to.Row == 0) || (!to.Piece.IsWhite && to.Row == 7))
            {
                to.Piece = new ChessPiece { Type = ChessPieceType.Queen, IsWhite = to.Piece.IsWhite };
            }
        }

        // Switch turn
        IsWhiteTurn = !IsWhiteTurn;
        ((RelayCommand)UndoMoveCommand).RaiseCanExecuteChanged();
    }

    private void AddCapturedPiece(ChessPiece piece)
    {
        string symbol = ChessPieceSymbols.GetSymbol(piece.Type, piece.IsWhite);

        if (piece.IsWhite)
        {
            BlackCapturedPieces += symbol;
            OnPropertyChanged(nameof(BlackCapturedPieces));
        }
        else
        {
            WhiteCapturedPieces += symbol;
            OnPropertyChanged(nameof(WhiteCapturedPieces));
        }
    }

    private void NewGame()
    {
        _aiMoveTimer.Stop();
        InitializeBoard();
    }

    private bool CanUndoMove()
    {
        return _moveHistory.Count > 0 && !IsAiThinking;
    }

    private void UndoMove()
    {
        if (_moveHistory.Count == 0 || IsAiThinking) return;

        // In AI mode, undo both player and AI move
        int movesToUndo = IsPlayingAgainstAi && _moveHistory.Count >= 2 ? 2 : 1;

        for (int i = 0; i < movesToUndo && _moveHistory.Count > 0; i++)
        {
            var lastMove = _moveHistory[^1];
            _moveHistory.RemoveAt(_moveHistory.Count - 1);

            var fromSquare = GetSquare(lastMove.FromRow, lastMove.FromCol);
            var toSquare = GetSquare(lastMove.ToRow, lastMove.ToCol);

            if (fromSquare != null && toSquare != null)
            {
                fromSquare.Piece = lastMove.MovedPiece;
                toSquare.Piece = lastMove.CapturedPiece;

                // Remove from captured if there was a capture
                if (lastMove.CapturedPiece != null)
                {
                    RemoveLastCapturedPiece(lastMove.CapturedPiece.IsWhite);
                }
            }

            IsWhiteTurn = !IsWhiteTurn;
        }

        GameStatus = string.Empty;
        ((RelayCommand)UndoMoveCommand).RaiseCanExecuteChanged();
    }

    private void RemoveLastCapturedPiece(bool wasWhite)
    {
        if (wasWhite && BlackCapturedPieces.Length > 0)
        {
            BlackCapturedPieces = BlackCapturedPieces[..^1];
            OnPropertyChanged(nameof(BlackCapturedPieces));
        }
        else if (!wasWhite && WhiteCapturedPieces.Length > 0)
        {
            WhiteCapturedPieces = WhiteCapturedPieces[..^1];
            OnPropertyChanged(nameof(WhiteCapturedPieces));
        }
    }

    private void ShowHint()
    {
        if (IsAiThinking) return;
        
        // Use the engine to suggest a move
        var bestMove = _engine.GetBestMove(Squares, IsWhiteTurn, ChessDifficulty.Medium);
        
        if (bestMove.HasValue)
        {
            DeselectAll();
            SelectSquare(bestMove.Value.from);
        }
    }

    private void SetDifficulty(string? difficultyStr)
    {
        if (Enum.TryParse<ChessDifficulty>(difficultyStr, out var difficulty))
        {
            Difficulty = difficulty;
        }
    }

    private void ToggleAi()
    {
        IsPlayingAgainstAi = !IsPlayingAgainstAi;
        NewGame();
    }

    #endregion
}

/// <summary>
/// ViewModel for individual chess squares.
/// </summary>
public sealed class ChessSquareViewModel : ViewModelBase
{
    private ChessPiece? _piece;
    private bool _isSelected;
    private bool _isValidMove;
    private Brush _background = Brushes.White;

    public int Row { get; init; }
    public int Column { get; init; }
    public Brush OriginalBackground { get; init; } = Brushes.White;

    public Brush Background
    {
        get => _background;
        set => SetProperty(ref _background, value);
    }

    public ChessPiece? Piece
    {
        get => _piece;
        set
        {
            if (SetProperty(ref _piece, value))
            {
                OnPropertyChanged(nameof(PieceSymbol));
            }
        }
    }

    public string PieceSymbol => Piece == null ? string.Empty : ChessPieceSymbols.GetSymbol(Piece.Type, Piece.IsWhite);

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public bool IsValidMove
    {
        get => _isValidMove;
        set => SetProperty(ref _isValidMove, value);
    }
}
