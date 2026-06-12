using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KatyaKatya.Localization;
using KatyaKatya.Models.Lobby;
using KatyaKatya.Services.Interfaces;
using KatyaKatya.ViewModels.MainMenu;

namespace KatyaKatya.ViewModels.SinglePlayer;

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
        "yumiko-1/yumiko-1-original",
        "akari-1/akari-1-original"
    ];

    private readonly INavigationService _navigation;
    private readonly ISessionService _session;
    private readonly IDialogService _dialog;

    private bool _isGameFinished;
    private bool _isProcessing;
    private CardViewModel? _firstFlipped;
    private int _totalTimeSeconds;
    private int _remainingTotalSeconds;
    private DispatcherTimer? _gameTimer;
    private int _elapsedSeconds;

    [ObservableProperty] private int _score;
    [ObservableProperty] private int _attempts;
    [ObservableProperty] private string _timeDisplay = "--";
    [ObservableProperty] private string _elapsedDisplay = "00:00";
    [ObservableProperty] private int _boardColumns = 4;
    [ObservableProperty] private int _boardRows = 4;
    [ObservableProperty] private bool _showGameOver;
    [ObservableProperty] private string _gameOverTitle = string.Empty;
    [ObservableProperty] private string _gameOverStats = string.Empty;

    /// <summary>Toggled off/on around each score change so the ScorePulse animation re-runs.</summary>
    [ObservableProperty] private bool _scorePulseActive;

    partial void OnScoreChanged(int value)
    {
        ScorePulseActive = false;
        Dispatcher.UIThread.Post(() => ScorePulseActive = true);
    }

    public bool IsInteractionEnabled => !_isProcessing && !_isGameFinished;

    public event Action? PairMatched;

    public ObservableCollection<CardViewModel> Cards { get; } = new();

    public string PlayerName => _session.Current?.Username ?? "Player";

    public string TimeLabel => _totalTimeSeconds > 0
        ? LocalizationManager.Instance.TryGet("SinglePlayer_Label_TimeLeft") ?? "Time Left"
        : LocalizationManager.Instance["SinglePlayer_Label_Elapsed"];

    public SinglePlayerGameViewModel(
        INavigationService navigation,
        ISessionService session,
        IDialogService dialog)
    {
        _navigation = navigation;
        _session = session;
        _dialog = dialog;
    }

    public void Initialize(int cardCount, int totalTimeSeconds)
    {
        _totalTimeSeconds = totalTimeSeconds;
        _remainingTotalSeconds = totalTimeSeconds;
        _isGameFinished = false;
        _isProcessing = false;
        _firstFlipped = null;
        _elapsedSeconds = 0;
        Score = 0;
        Attempts = 0;
        ShowGameOver = false;

        _gameTimer?.Stop();

        Cards.Clear();
        foreach (var (index, imageId) in GenerateBoard(cardCount))
        {
            var card = new CardViewModel(index) { ImageIdentifier = imageId };
            Cards.Add(card);
        }

        CalculateBoardSize(cardCount);
        UpdateInteraction();
        ElapsedDisplay = FormatTime(_elapsedSeconds);
        SetTimeDisplay();
        OnPropertyChanged(nameof(TimeLabel));
        StartGameTimer();
    }

    [RelayCommand]
    private async Task FlipCardAsync(CardViewModel? card)
    {
        if (card is null || _isProcessing || _isGameFinished || card.IsFlipped || card.IsMatched)
            return;

        if (_firstFlipped is null)
        {
            _firstFlipped = card;
            card.IsFlipped = true;
            return;
        }

        card.IsFlipped = true;
        Attempts++;

        var first = _firstFlipped;
        _firstFlipped = null;
        _isProcessing = true;
        UpdateInteraction();

        await Task.Delay(700);

        if (_isGameFinished)
        {
            _isProcessing = false;
            UpdateInteraction();
            return;
        }

        if (first.ImageIdentifier == card.ImageIdentifier)
        {
            first.IsMatched = true;
            card.IsMatched = true;
            Score++;
            PairMatched?.Invoke();

            if (Cards.All(c => c.IsMatched))
                FinishGame(completed: true);
        }
        else
        {
            first.IsFlipped = false;
            card.IsFlipped = false;
        }

        _isProcessing = false;
        UpdateInteraction();
    }

    [RelayCommand]
    private void LeaveGame()
    {
        var result = _dialog.ShowMessage(
            LocalizationManager.Instance["Lobby_Message_LeaveLobby"],
            LocalizationManager.Instance["Global_Title_Confirm"],
            DialogButton.YesNo, DialogIcon.Question);

        if (result != DialogResult.Yes) return;

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

    private void StartGameTimer()
    {
        _gameTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _gameTimer.Tick += (_, _) =>
        {
            _elapsedSeconds++;
            ElapsedDisplay = FormatTime(_elapsedSeconds);

            if (_totalTimeSeconds > 0)
            {
                _remainingTotalSeconds--;
                SetTimeDisplay();

                if (_remainingTotalSeconds <= 0)
                    FinishGame(completed: false);
            }
            else
            {
                SetTimeDisplay();
            }
        };
        _gameTimer.Start();
    }

    private void SetTimeDisplay()
        => TimeDisplay = _totalTimeSeconds > 0
            ? FormatTime(Math.Max(0, _remainingTotalSeconds))
            : ElapsedDisplay;

    private void FinishGame(bool completed)
    {
        _isGameFinished = true;
        StopAllTimers();
        UpdateInteraction();

        int totalPairs = Cards.Count / 2;

        GameOverTitle = completed
            ? LocalizationManager.Instance["SinglePlayer_Label_GameOver_Complete"]
            : LocalizationManager.Instance.TryGet("SinglePlayer_Label_GameOver_TimeUp") ?? "Time's up!";
        GameOverStats = LocalizationManager.Instance.Format(
            "SinglePlayer_Label_GameOver_Stats",
            Score,
            totalPairs,
            Attempts,
            ElapsedDisplay);

        ShowGameOver = true;
    }

    private void UpdateInteraction()
        => OnPropertyChanged(nameof(IsInteractionEnabled));

    private void StopAllTimers()
        => _gameTimer?.Stop();

    private void CalculateBoardSize(int cardCount)
    {
        int rows = (int)Math.Sqrt(cardCount);
        while (rows > 1 && cardCount % rows != 0)
            rows--;

        int cols = cardCount / rows;
        BoardColumns = Math.Max(rows, cols);
        BoardRows = Math.Min(rows, cols);
    }

    private static string FormatTime(int seconds)
        => TimeSpan.FromSeconds(seconds).ToString(@"mm\:ss");

    private static List<(int Index, string ImageId)> GenerateBoard(int cardCount)
    {
        int pairCount = cardCount / 2;
        var pairs = new List<string>(cardCount);

        for (int i = 0; i < pairCount; i++)
        {
            var img = AvailableImages[i % AvailableImages.Count];
            pairs.Add(img);
            pairs.Add(img);
        }

        for (int i = pairs.Count - 1; i > 0; i--)
        {
            int j = Rng.Next(i + 1);
            (pairs[i], pairs[j]) = (pairs[j], pairs[i]);
        }

        return pairs.Select((img, idx) => (idx, img)).ToList();
    }
}
