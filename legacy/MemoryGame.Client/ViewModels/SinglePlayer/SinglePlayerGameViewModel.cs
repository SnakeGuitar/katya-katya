using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Localization;
using MemoryGame.Client.Models.Lobby;
using MemoryGame.Client.Services.Interfaces;
using MemoryGame.Client.ViewModels.MainMenu;

namespace MemoryGame.Client.ViewModels.SinglePlayer;

/// <summary>
/// Pure client-side singleplayer game. No server connection required.
/// Replicates the card/match logic from the server's GameSession entirely on the client.
/// </summary>
public partial class SinglePlayerGameViewModel : ObservableObject
{
    private static readonly Random Rng = new();

    private static readonly List<string> AvailableImages =
    [
        "katya-1/katya-1-no-background",
        "katya-moods/main/katya-main-no-background",
        "katya-moods/in-love/katya-in-love-no-background",
        "katya-moods/shy/katya-shy-2-no-background",
        "katya-moods/sitting/katya-sit-no-background",
        "katya-moods/standing/sketch-katya-standing-no-background",
        "yumiko-1/yumiko-1-original",
        "akari-1/akari-1-original",
        "katya-moods/happy/katya-happy",
        "katya-moods/shy/katya-shy-3",
        "katya-1/katya-1-original-border",
        "katya-moods/main/sketch-katya-main-no-background",
        "katya-moods/in-love/sketch-katya-in-love-no-background",
        "katya-moods/shy/sketch-katya-shy-no-background",
        "katya-moods/happy/katya-happy",
        "katya-moods/shy/katya-shy-3",
        "yumiko-1/yumiko-1-original",
        "akari-1/akari-1-original",
    ];

    private readonly INavigationService _navigation;
    private readonly ISessionService    _session;
    private readonly IDialogService     _dialog;

    // ── Game state ────────────────────────────────────────────────────────────

    private bool            _isGameFinished;
    private bool            _isProcessing;     // locked while handling a pair reveal
    private CardViewModel?  _firstFlipped;
    private int             _turnTimeSeconds;  // 0 = unlimited
    private int             _remainingTurnSeconds;
    private DispatcherTimer? _turnTimer;
    private DispatcherTimer? _elapsedTimer;
    private int             _elapsedSeconds;

    // ── Observable properties ─────────────────────────────────────────────────

    [ObservableProperty] private int    _score;                               // pairs found
    [ObservableProperty] private int    _attempts;                            // pair flip attempts
    [ObservableProperty] private string _turnTimerDisplay = "--";
    [ObservableProperty] private string _elapsedDisplay   = "00:00";
    [ObservableProperty] private int    _boardColumns     = 4;
    [ObservableProperty] private int    _boardRows        = 4;
    [ObservableProperty] private bool   _showGameOver;
    [ObservableProperty] private string _gameOverTitle    = string.Empty;
    [ObservableProperty] private string _gameOverStats    = string.Empty;

    /// <summary>True while cards are NOT being processed — used by the view to enable/disable card buttons.</summary>
    public bool IsInteractionEnabled => !_isProcessing && !_isGameFinished;

    /// <summary>Fired on the UI thread whenever a pair is successfully matched. The view uses this to spawn particles.</summary>
    public event Action? PairMatched;

    public ObservableCollection<CardViewModel> Cards { get; } = new();

    public string PlayerName => _session.Current?.Username ?? "Player";

    // ── Constructor ───────────────────────────────────────────────────────────

    public SinglePlayerGameViewModel(
        INavigationService navigation,
        ISessionService    session,
        IDialogService     dialog)
    {
        _navigation = navigation;
        _session    = session;
        _dialog     = dialog;
    }

    // ── Initialization ────────────────────────────────────────────────────────

    public void Initialize(int cardCount, int turnTimeSeconds)
    {
        _turnTimeSeconds  = turnTimeSeconds;
        _isGameFinished   = false;
        _isProcessing     = false;
        _firstFlipped     = null;
        _elapsedSeconds   = 0;
        Score             = 0;
        Attempts          = 0;
        ShowGameOver      = false;

        _turnTimer?.Stop();
        _elapsedTimer?.Stop();

        Cards.Clear();
        foreach (var (index, imageId) in GenerateBoard(cardCount))
        {
            var card = new CardViewModel(index) { ImageIdentifier = imageId };
            Cards.Add(card);
        }

        CalculateBoardSize(cardCount);
        UpdateInteraction();
        SetTurnTimerDisplay();
        StartElapsedTimer();
    }

    // ── Card flip command ─────────────────────────────────────────────────────

    [RelayCommand]
    private async Task FlipCardAsync(CardViewModel? card)
    {
        if (card is null || _isProcessing || _isGameFinished || card.IsFlipped || card.IsMatched)
            return;

        if (_firstFlipped is null)
        {
            // First card of a pair
            _firstFlipped = card;
            card.IsFlipped = true;
            StartTurnTimer();
            return;
        }

        // Second card of the pair
        card.IsFlipped = true;
        Attempts++;

        var first  = _firstFlipped;
        _firstFlipped = null;
        _isProcessing = true;
        UpdateInteraction();
        StopTurnTimer();

        // Brief pause so the player can see both cards before they flip back
        await Task.Delay(700);

        if (first.ImageIdentifier == card.ImageIdentifier)
        {
            first.IsMatched = true;
            card.IsMatched  = true;
            Score++;
            PairMatched?.Invoke();

            if (Cards.All(c => c.IsMatched))
                FinishGame(completed: true);
        }
        else
        {
            first.IsFlipped = false;
            card.IsFlipped  = false;
        }

        _isProcessing = false;
        UpdateInteraction();

        if (!_isGameFinished)
            SetTurnTimerDisplay();
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    [RelayCommand]
    private void LeaveGame()
    {
        var result = _dialog.ShowMessage(
            LocalizationManager.Instance.TryGet("Global_Message_ExitGame") ?? "Are you sure you want to leave?",
            LocalizationManager.Instance.TryGet("Global_Title_Confirm")    ?? "Confirm",
            DialogButton.YesNo, DialogIcon.Question);

        if (result != Services.Interfaces.DialogResult.Yes) return;

        StopAllTimers();
        _navigation.NavigateToRoot<MainMenuViewModel>();
    }

    [RelayCommand]
    private void CloseGameOver()
    {
        ShowGameOver = false;
        StopAllTimers();
        _navigation.NavigateTo<SinglePlayerMenuViewModel>();
    }

    [RelayCommand]
    private void PlayAgain()
    {
        ShowGameOver = false;
        StopAllTimers();
        _navigation.NavigateTo<SinglePlayerMenuViewModel>();
    }

    // ── Turn timer ────────────────────────────────────────────────────────────

    private void StartTurnTimer()
    {
        if (_turnTimeSeconds <= 0)
        {
            TurnTimerDisplay = "∞";
            return;
        }

        _remainingTurnSeconds = _turnTimeSeconds;
        TurnTimerDisplay = FormatTime(_remainingTurnSeconds);

        _turnTimer?.Stop();
        _turnTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _turnTimer.Tick += OnTurnTimerTick;
        _turnTimer.Start();
    }

    private void StopTurnTimer()
    {
        _turnTimer?.Stop();
        SetTurnTimerDisplay();
    }

    private void OnTurnTimerTick(object? sender, EventArgs e)
    {
        _remainingTurnSeconds--;
        TurnTimerDisplay = FormatTime(Math.Max(0, _remainingTurnSeconds));

        if (_remainingTurnSeconds > 0) return;

        // Time expired — flip back the first card if it's still waiting
        _turnTimer?.Stop();

        if (_firstFlipped is not null)
        {
            _firstFlipped.IsFlipped = false;
            _firstFlipped = null;
        }

        SetTurnTimerDisplay();
    }

    private void SetTurnTimerDisplay()
        => TurnTimerDisplay = _turnTimeSeconds > 0 ? FormatTime(_turnTimeSeconds) : "∞";

    // ── Elapsed timer ─────────────────────────────────────────────────────────

    private void StartElapsedTimer()
    {
        _elapsedTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _elapsedTimer.Tick += (_, _) =>
        {
            _elapsedSeconds++;
            ElapsedDisplay = FormatTime(_elapsedSeconds);
        };
        _elapsedTimer.Start();
    }

    // ── Game over ─────────────────────────────────────────────────────────────

    private void FinishGame(bool completed)
    {
        _isGameFinished = true;
        StopAllTimers();
        UpdateInteraction();

        int totalPairs = Cards.Count / 2;

        GameOverTitle = LocalizationManager.Instance.TryGet("SinglePlayer_Label_GameOver_Complete")
                        ?? "Puzzle Complete!";

        GameOverStats = string.Format(
            LocalizationManager.Instance.TryGet("SinglePlayer_Label_GameOver_Stats") ?? "Pairs: {0}/{1}  ·  Attempts: {2}  ·  Time: {3}",
            Score, totalPairs, Attempts, ElapsedDisplay);

        ShowGameOver = true;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void UpdateInteraction()
        => OnPropertyChanged(nameof(IsInteractionEnabled));

    private void StopAllTimers()
    {
        _turnTimer?.Stop();
        _elapsedTimer?.Stop();
    }

    private void CalculateBoardSize(int cardCount)
    {
        int rows = (int)Math.Sqrt(cardCount);
        while (rows > 1 && cardCount % rows != 0)
            rows--;

        int cols    = cardCount / rows;
        BoardColumns = Math.Max(rows, cols);
        BoardRows    = Math.Min(rows, cols);
    }

    private static string FormatTime(int seconds)
        => TimeSpan.FromSeconds(seconds).ToString(@"mm\:ss");

    private static List<(int Index, string ImageId)> GenerateBoard(int cardCount)
    {
        int pairCount = cardCount / 2;
        var pairs     = new List<string>(cardCount);

        for (int i = 0; i < pairCount; i++)
        {
            var img = AvailableImages[i % AvailableImages.Count];
            pairs.Add(img);
            pairs.Add(img);
        }

        // Fisher-Yates shuffle
        for (int i = pairs.Count - 1; i > 0; i--)
        {
            int j = Rng.Next(i + 1);
            (pairs[i], pairs[j]) = (pairs[j], pairs[i]);
        }

        return pairs.Select((img, idx) => (idx, img)).ToList();
    }
}
