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
using MemoryGame.Client.Services.Network;

namespace MemoryGame.Client.ViewModels.Lobby;

/// <summary>
/// ViewModel for the multiplayer game board screen.
/// Handles card state, turns, scores, chat, player leaving, and vote-to-kick during gameplay.
/// Migrated from the legacy PlayGameMultiplayer code-behind to proper MVVM.
/// </summary>
public partial class GameBoardViewModel : ObservableObject
{
    private const int MaxChatMessages = 100;

    private readonly INavigationService _navigation;
    private readonly ISessionService _session;
    private readonly ILobbyService _lobbyService;
    private readonly IChatService _chatService;
    private readonly IGameService _gameService;
    private readonly IDialogService _dialog;
    private readonly HubService _hub;

    private bool _disposed;
    private bool _isGameFinished;
    private DispatcherTimer? _turnTimer;
    private int _remainingSeconds;

    // ── Observable Properties ─────────────────────────────────────────────

    [ObservableProperty]
    private string _chatMessage = string.Empty;

    [ObservableProperty]
    private string _currentTurnPlayer = string.Empty;

    [ObservableProperty]
    private bool _isMyTurn;

    [ObservableProperty]
    private string _winnerName = string.Empty;

    [ObservableProperty]
    private bool _showGameOver;

    [ObservableProperty]
    private string _gameOverTitle = string.Empty;

    [ObservableProperty]
    private string _gameOverStats = string.Empty;

    public ObservableCollection<CardViewModel> Cards { get; } = new();
    public ObservableCollection<PlayerScoreViewModel> Players { get; } = new();
    public ObservableCollection<string> ChatMessages { get; } = new();

    public string CurrentUsername => _session.Current?.Username ?? "Player";

    /// <summary>
    /// Number of columns for the UniformGrid. Calculated from card count.
    /// </summary>
    [ObservableProperty]
    private int _boardColumns = 4;

    [ObservableProperty]
    private int _boardRows = 4;

    /// <summary>
    /// Fired when the chat needs to scroll to the newest message.
    /// </summary>
    public event Action? ScrollChatToBottom;

    // ── Constructor ───────────────────────────────────────────────────────

    public GameBoardViewModel(
        INavigationService navigation,
        ISessionService session,
        ILobbyService lobbyService,
        IChatService chatService,
        IGameService gameService,
        IDialogService dialog,
        HubService hub)
    {
        _navigation = navigation;
        _session = session;
        _lobbyService = lobbyService;
        _chatService = chatService;
        _gameService = gameService;
        _dialog = dialog;
        _hub = hub;
    }

    /// <summary>
    /// Initializes the board from the server's GameStarted event data.
    /// Call this from the lobby VM before navigating here.
    /// </summary>
    public void Initialize(List<CardInfoDto> boardCards, List<LobbyPlayerDto> players)
    {
        Cards.Clear();
        foreach (var card in boardCards)
        {
            Cards.Add(new CardViewModel(card.Index));
        }

        Players.Clear();
        foreach (var p in players)
        {
            Players.Add(new PlayerScoreViewModel(p.Username, p.Username == CurrentUsername));
        }

        CalculateBoardSize(boardCards.Count);
        SubscribeEvents();
    }

    private void CalculateBoardSize(int cardCount)
    {
        // Find the best factors to create a balanced grid (closer to square)
        // We start from the square root and work downwards to find the largest factor
        int rows = (int)Math.Sqrt(cardCount);
        while (rows > 1 && cardCount % rows != 0)
        {
            rows--;
        }

        int cols = cardCount / rows;

        // Prefer landscape (more columns than rows)
        BoardColumns = Math.Max(rows, cols);
        BoardRows = Math.Min(rows, cols);
    }

    // ── Event Wiring ──────────────────────────────────────────────────────

    private void SubscribeEvents()
    {
        _gameService.TurnUpdated += OnTurnUpdated;
        _gameService.CardShown += OnCardShown;
        _gameService.CardsMatched += OnCardsMatched;
        _gameService.CardsHidden += OnCardsHidden;
        _gameService.ScoreUpdated += OnScoreUpdated;
        _gameService.GameFinished += OnGameFinished;
        _chatService.MessageReceived += OnChatMessageReceived;
        _lobbyService.PlayerLeft += OnPlayerLeft;
        _lobbyService.Kicked += OnKicked;
    }

    private void UnsubscribeEvents()
    {
        if (_disposed) return;
        _disposed = true;

        _gameService.TurnUpdated -= OnTurnUpdated;
        _gameService.CardShown -= OnCardShown;
        _gameService.CardsMatched -= OnCardsMatched;
        _gameService.CardsHidden -= OnCardsHidden;
        _gameService.ScoreUpdated -= OnScoreUpdated;
        _gameService.GameFinished -= OnGameFinished;
        _chatService.MessageReceived -= OnChatMessageReceived;
        _lobbyService.PlayerLeft -= OnPlayerLeft;
        _lobbyService.Kicked -= OnKicked;

        _turnTimer?.Stop();
    }

    // ── Server Event Handlers ─────────────────────────────────────────────

    private void OnTurnUpdated(string nextPlayer, int seconds)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            CurrentTurnPlayer = nextPlayer;
            IsMyTurn = string.Equals(
                nextPlayer?.Trim(),
                CurrentUsername?.Trim(),
                StringComparison.OrdinalIgnoreCase);
            DebugLog(
                $"[GameBoard] TurnUpdated: nextPlayer='{nextPlayer}' me='{CurrentUsername}' isMyTurn={IsMyTurn}");

            foreach (var p in Players)
            {
                p.IsActive = p.Username == nextPlayer;
                p.TimeDisplay = p.Username == nextPlayer ? FormatTime(seconds) : "--";
            }

            StartLocalTurnTimer(seconds);
        });
    }

    private void OnCardShown(int cardIndex, string? imageId)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            var card = Cards.FirstOrDefault(c => c.Index == cardIndex);
            if (card is null) return;

            card.ImageIdentifier = imageId;
            card.IsFlipped = true;
        });
    }

    private void OnCardsMatched(int idx1, int idx2)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            var c1 = Cards.FirstOrDefault(c => c.Index == idx1);
            var c2 = Cards.FirstOrDefault(c => c.Index == idx2);

            if (c1 is not null) { c1.IsMatched = true; c1.IsFlipped = true; }
            if (c2 is not null) { c2.IsMatched = true; c2.IsFlipped = true; }
        });
    }

    private void OnCardsHidden(int idx1, int idx2)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            var c1 = Cards.FirstOrDefault(c => c.Index == idx1);
            var c2 = Cards.FirstOrDefault(c => c.Index == idx2);

            if (c1 is not null) { c1.IsFlipped = false; }
            if (c2 is not null) { c2.IsFlipped = false; }
        });
    }

    private void OnScoreUpdated(string playerName, int score)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            var player = Players.FirstOrDefault(p => p.Username == playerName);
            if (player is not null)
                player.Score = score;
        });
    }

    private void OnGameFinished(string? winner)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            _isGameFinished = true;
            _turnTimer?.Stop();

            string title;
            if (string.IsNullOrEmpty(winner))
                title = LocalizationManager.Instance["GameBoard_Label_Tie"] ?? "Tie!";
            else
                title = string.Format(
                    LocalizationManager.Instance["GameBoard_Label_Winner"] ?? "{0} wins!",
                    winner);

            var scoreSummary = string.Join("\n",
                Players.OrderByDescending(p => p.Score)
                       .Select(p => $"{p.Username}: {p.Score}"));

            GameOverTitle = title;
            GameOverStats = scoreSummary;
            ShowGameOver = true;
        });
    }

    private void OnChatMessageReceived(string sender, string message, bool isSystem)
    {
        if (_disposed) return;

        App.Current.Dispatcher.Invoke(() =>
        {
            string formatted = isSystem ? $"⸻ {message} ⸻" : $"{sender}: {message}";
            ChatMessages.Add(formatted);

            while (ChatMessages.Count > MaxChatMessages)
                ChatMessages.RemoveAt(0);

            ScrollChatToBottom?.Invoke();
        });
    }

    private void OnPlayerLeft(string username)
    {
        if (_disposed) return;

        App.Current.Dispatcher.Invoke(() =>
        {
            string msg = $"⸻ {username} left the game ⸻";
            ChatMessages.Add(msg);
            ScrollChatToBottom?.Invoke();

            var player = Players.FirstOrDefault(p => p.Username == username);
            if (player is not null)
                Players.Remove(player);
        });
    }

    private void OnKicked()
    {
        if (_disposed) return;

        App.Current.Dispatcher.Invoke(() =>
        {
            UnsubscribeEvents();
            _dialog.ShowMessage(
                LocalizationManager.Instance["Lobby_Message_Kicked"] ?? "You have been kicked.",
                LocalizationManager.Instance["Global_Title_Information"] ?? "Information",
                DialogButton.OK, DialogIcon.Information);
            _navigation.NavigateToRoot<LobbyMenuViewModel>();
        });
    }

    // ── Local Timer ───────────────────────────────────────────────────────

    private void StartLocalTurnTimer(int seconds)
    {
        _turnTimer?.Stop();
        _remainingSeconds = seconds;

        _turnTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _turnTimer.Tick += (_, _) =>
        {
            _remainingSeconds--;
            if (_remainingSeconds < 0) _remainingSeconds = 0;

            var activePlayer = Players.FirstOrDefault(p => p.IsActive);
            if (activePlayer is not null)
                activePlayer.TimeDisplay = FormatTime(_remainingSeconds);

            if (_remainingSeconds <= 0)
                _turnTimer?.Stop();
        };
        _turnTimer.Start();
    }

    private static string FormatTime(int seconds)
    {
        var t = TimeSpan.FromSeconds(seconds);
        return t.ToString(@"mm\:ss");
    }

    // ── Commands ──────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task FlipCardAsync(CardViewModel? card)
    {
        if (card is null) return;

        if (!IsMyTurn || card.IsFlipped || card.IsMatched || _isGameFinished)
        {
            DebugLog(
                $"[GameBoard] FlipCard ignored: index={card.Index} isMyTurn={IsMyTurn} flipped={card.IsFlipped} matched={card.IsMatched} finished={_isGameFinished}");
            return;
        }

        try
        {
            DebugLog($"[GameBoard] FlipCard request: index={card.Index}");
            await _gameService.FlipCardAsync(card.Index);
        }
        catch (Exception ex)
        {
            DebugLog($"[GameBoard] FlipCard failed: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task SendChatMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(ChatMessage)) return;

        try
        {
            await _chatService.SendChatMessageAsync(ChatMessage);
            ChatMessage = string.Empty;
        }
        catch
        {
            // Silent
        }
    }

    [RelayCommand]
    private async Task VoteToKickAsync(PlayerScoreViewModel? player)
    {
        if (player is null || player.IsCurrentUser) return;

        var result = _dialog.ShowMessage(
            string.Format(
                LocalizationManager.Instance["KickVote_Message_VoteKickPlayer"] ?? "Vote to kick {0}?",
                player.Username),
            LocalizationManager.Instance["Global_Title_Confirm"] ?? "Confirm",
            DialogButton.YesNo, DialogIcon.Question);

        if (result != Services.Interfaces.DialogResult.Yes) return;

        try
        {
            await _lobbyService.VoteToKickAsync(player.Username);
        }
        catch
        {
            // Best-effort
        }
    }

    [RelayCommand]
    private async Task LeaveGameAsync()
    {
        var result = _dialog.ShowMessage(
            LocalizationManager.Instance["Global_Message_ExitGame"] ?? "Are you sure you want to leave?",
            LocalizationManager.Instance["Global_Title_Confirm"] ?? "Confirm",
            DialogButton.YesNo, DialogIcon.Question);

        if (result != Services.Interfaces.DialogResult.Yes) return;

        UnsubscribeEvents();

        try
        {
            await _lobbyService.LeaveLobbyAsync();
        }
        catch
        {
            // Best-effort
        }

        _navigation.NavigateToRoot<LobbyMenuViewModel>();
    }

    [RelayCommand]
    private void CloseGameOver()
    {
        ShowGameOver = false;
        UnsubscribeEvents();
        _ = SafeLeaveAsync();
        _navigation.NavigateToRoot<LobbyMenuViewModel>();
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static readonly string LogPath =
        System.IO.Path.Combine(System.IO.Path.GetTempPath(), "memorygame-client.log");

    private static void DebugLog(string msg)
    {
        try
        {
            System.IO.File.AppendAllText(LogPath, $"{DateTime.Now:HH:mm:ss.fff} {msg}{Environment.NewLine}");
        }
        catch { }
    }

    private async Task SafeLeaveAsync()
    {
        try
        {
            await _lobbyService.LeaveLobbyAsync();
        }
        catch
        {
            // Ignore
        }
    }
}
