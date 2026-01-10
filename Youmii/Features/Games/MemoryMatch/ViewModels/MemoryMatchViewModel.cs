using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using Youmii.Features.Games.MemoryMatch.Models;
using Youmii.ViewModels;

namespace Youmii.Features.Games.MemoryMatch.ViewModels;

/// <summary>
/// ViewModel for Memory Match game.
/// </summary>
public sealed class MemoryMatchViewModel : ViewModelBase
{
    private readonly Random _random = new();
    private readonly DispatcherTimer _flipBackTimer;
    private readonly DispatcherTimer _gameTimer;
    private MemoryCardViewModel? _firstFlipped;
    private MemoryCardViewModel? _secondFlipped;
    private bool _isProcessing;
    private int _moves;
    private int _matches;
    private int _elapsedSeconds;
    private int _bestTime = int.MaxValue;
    private int _bestMoves = int.MaxValue;

    public MemoryMatchViewModel()
    {
        Cards = [];
        
        CardClickedCommand = new RelayCommand<MemoryCardViewModel>(OnCardClicked);
        NewGameCommand = new RelayCommand(NewGame);

        _flipBackTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(800) };
        _flipBackTimer.Tick += OnFlipBackTimerTick;

        _gameTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _gameTimer.Tick += OnGameTimerTick;

        InitializeBoard();
    }

    public ObservableCollection<MemoryCardViewModel> Cards { get; }

    public int Moves
    {
        get => _moves;
        private set => SetProperty(ref _moves, value);
    }

    public int Matches
    {
        get => _matches;
        private set
        {
            if (SetProperty(ref _matches, value))
            {
                OnPropertyChanged(nameof(IsGameComplete));
                OnPropertyChanged(nameof(StatusText));
            }
        }
    }

    public int ElapsedSeconds
    {
        get => _elapsedSeconds;
        private set
        {
            if (SetProperty(ref _elapsedSeconds, value))
            {
                OnPropertyChanged(nameof(TimeDisplay));
            }
        }
    }

    public string TimeDisplay
    {
        get
        {
            var minutes = ElapsedSeconds / 60;
            var seconds = ElapsedSeconds % 60;
            return $"{minutes:D2}:{seconds:D2}";
        }
    }

    public int BestTime
    {
        get => _bestTime;
        private set
        {
            if (SetProperty(ref _bestTime, value))
            {
                OnPropertyChanged(nameof(BestTimeDisplay));
            }
        }
    }

    public string BestTimeDisplay => BestTime == int.MaxValue 
        ? "--:--" 
        : $"{BestTime / 60:D2}:{BestTime % 60:D2}";

    public int BestMoves
    {
        get => _bestMoves;
        private set
        {
            if (SetProperty(ref _bestMoves, value))
            {
                OnPropertyChanged(nameof(BestMovesDisplay));
            }
        }
    }

    public string BestMovesDisplay => BestMoves == int.MaxValue ? "--" : BestMoves.ToString();

    public bool IsGameComplete => Matches == 8;

    public string StatusText => IsGameComplete ? "?? Complete!" : $"{Matches}/8 Pairs";

    public ICommand CardClickedCommand { get; }
    public ICommand NewGameCommand { get; }

    public event EventHandler? RequestClose;

    private void InitializeBoard()
    {
        Cards.Clear();
        _firstFlipped = null;
        _secondFlipped = null;
        _isProcessing = false;
        Moves = 0;
        Matches = 0;
        ElapsedSeconds = 0;
        _gameTimer.Stop();

        var cardList = new List<MemoryCardViewModel>();
        int id = 0;

        for (int i = 0; i < CardSymbols.Pairs.Length; i++)
        {
            var (symbol, color) = CardSymbols.Pairs[i];
            
            // Add two cards for each pair
            cardList.Add(new MemoryCardViewModel
            {
                Id = id++,
                PairId = i,
                Symbol = symbol,
                Color = color
            });
            cardList.Add(new MemoryCardViewModel
            {
                Id = id++,
                PairId = i,
                Symbol = symbol,
                Color = color
            });
        }

        // Shuffle
        var shuffled = cardList.OrderBy(_ => _random.Next()).ToList();
        
        foreach (var card in shuffled)
        {
            Cards.Add(card);
        }
    }

    private void OnCardClicked(MemoryCardViewModel? card)
    {
        if (card == null || card.IsFlipped || card.IsMatched || _isProcessing) return;

        // Start timer on first move
        if (Moves == 0 && _firstFlipped == null)
        {
            _gameTimer.Start();
        }

        card.IsFlipped = true;

        if (_firstFlipped == null)
        {
            _firstFlipped = card;
        }
        else if (_secondFlipped == null)
        {
            _secondFlipped = card;
            Moves++;
            _isProcessing = true;

            // Check for match
            if (_firstFlipped.PairId == _secondFlipped.PairId)
            {
                _firstFlipped.IsMatched = true;
                _secondFlipped.IsMatched = true;
                Matches++;
                
                _firstFlipped = null;
                _secondFlipped = null;
                _isProcessing = false;

                if (IsGameComplete)
                {
                    _gameTimer.Stop();
                    UpdateBestScores();
                }
            }
            else
            {
                _flipBackTimer.Start();
            }
        }
    }

    private void OnFlipBackTimerTick(object? sender, EventArgs e)
    {
        _flipBackTimer.Stop();
        
        if (_firstFlipped != null)
            _firstFlipped.IsFlipped = false;
        if (_secondFlipped != null)
            _secondFlipped.IsFlipped = false;

        _firstFlipped = null;
        _secondFlipped = null;
        _isProcessing = false;
    }

    private void OnGameTimerTick(object? sender, EventArgs e)
    {
        ElapsedSeconds++;
    }

    private void UpdateBestScores()
    {
        if (ElapsedSeconds < BestTime)
            BestTime = ElapsedSeconds;
        if (Moves < BestMoves)
            BestMoves = Moves;
    }

    private void NewGame()
    {
        _flipBackTimer.Stop();
        _gameTimer.Stop();
        InitializeBoard();
    }
}

/// <summary>
/// ViewModel for individual memory cards.
/// </summary>
public sealed class MemoryCardViewModel : ViewModelBase
{
    private bool _isFlipped;
    private bool _isMatched;

    public int Id { get; init; }
    public int PairId { get; init; }
    public string Symbol { get; init; } = string.Empty;
    public string Color { get; init; } = "#FF9C27B0";

    public bool IsFlipped
    {
        get => _isFlipped;
        set => SetProperty(ref _isFlipped, value);
    }

    public bool IsMatched
    {
        get => _isMatched;
        set => SetProperty(ref _isMatched, value);
    }
}
