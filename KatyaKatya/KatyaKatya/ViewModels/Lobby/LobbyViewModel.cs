using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KatyaKatya.Helpers;
using KatyaKatya.Models.Lobby;
using KatyaKatya.Services.Interfaces;
using KatyaKatya.Services.Network;

namespace KatyaKatya.ViewModels.Lobby;

/// <summary>
/// ViewModel for the player (non-host) lobby screen in Avalonia.
/// The player can see the player list, use the chat, vote-kick, and wait for the host to start.
/// </summary>
public partial class LobbyViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ISessionService _session;
    private readonly ILobbyService _lobbyService;
    private readonly IChatService _chatService;
    private readonly IGameService _gameService;
    private readonly IDialogService _dialog;
    private readonly HubService _hub;

    private bool _isGameStarting;
    private bool _disposed;

    // ── Observable Properties ──────────────────────────────────────────────

    [ObservableProperty]
    private string _gameCode = string.Empty;

    [ObservableProperty]
    private string _chatMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasPlayers;

    public ObservableCollection<LobbyPlayerDto> Players { get; } = new();
    public ObservableCollection<string> ChatMessages { get; } = new();

    public bool IsGuest => _session.Current?.IsGuest == true;
    public string CurrentUsername => _session.Current?.Username ?? "Player";

    /// <summary>
    /// Fired when the chat needs to scroll to the newest message.
    /// The View subscribes to this to scroll its ScrollViewer.
    /// </summary>
    public event Action? ScrollChatToBottom;

    // ── Constructor ────────────────────────────────────────────────────────

    public LobbyViewModel(
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

        // Initialize with currently known players on the UI thread
        foreach (var p in _lobbyService.CurrentPlayers)
            Players.Add(p);

        HasPlayers = Players.Count > 0;

        SubscribeEvents();
    }

    // ── Event Wiring ───────────────────────────────────────────────────────

    private void SubscribeEvents()
    {
        _lobbyService.PlayerListUpdated += OnPlayerListUpdated;
        _lobbyService.PlayerJoined += OnPlayerJoined;
        _lobbyService.PlayerLeft += OnPlayerLeft;
        _lobbyService.Kicked += OnKicked;
        _lobbyService.ErrorReceived += OnErrorReceived;
        _chatService.MessageReceived += OnChatMessageReceived;
        _gameService.GameStarted += OnGameStarted;
    }

    private void UnsubscribeEvents()
    {
        if (_disposed) return;
        _disposed = true;

        _lobbyService.PlayerListUpdated -= OnPlayerListUpdated;
        _lobbyService.PlayerJoined -= OnPlayerJoined;
        _lobbyService.PlayerLeft -= OnPlayerLeft;
        _lobbyService.Kicked -= OnKicked;
        _lobbyService.ErrorReceived -= OnErrorReceived;
        _chatService.MessageReceived -= OnChatMessageReceived;
        _gameService.GameStarted -= OnGameStarted;
    }

    // ── Server Event Handlers ──────────────────────────────────────────────

    private void OnPlayerListUpdated(List<LobbyPlayerDto> players)
    {
        if (_isGameStarting || _disposed) return;

        Dispatcher.UIThread.Invoke(() =>
        {
            Players.Clear();
            foreach (var player in players)
                Players.Add(player);

            HasPlayers = Players.Count > 0;
        });
    }

    private void OnPlayerJoined(string username, bool isGuest)
    {
        if (_isGameStarting || _disposed) return;

        Dispatcher.UIThread.Invoke(() =>
        {
            string message = $"{username} joined the lobby";
            AddSystemMessage(message);
        });
    }

    private void OnPlayerLeft(string username)
    {
        if (_isGameStarting || _disposed) return;

        Dispatcher.UIThread.Invoke(() =>
        {
            string message = $"{username} left the lobby";
            AddSystemMessage(message);
        });
    }

    private void OnChatMessageReceived(string sender, string message, bool isSystem)
    {
        if (_isGameStarting || _disposed) return;

        Dispatcher.UIThread.Invoke(() =>
        {
            string formatted = isSystem ? $"⸻ {message} ⸻" : $"{sender}: {message}";
            ChatMessages.Add(formatted);
            ScrollChatToBottom?.Invoke();
        });
    }

    private void OnGameStarted(List<CardInfoDto> cards)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            _isGameStarting = true;
            var playersSnapshot = Players.ToList();
            UnsubscribeEvents();

            _navigation.NavigateTo<GameBoardViewModel>(vm =>
            {
                vm.Initialize(cards, playersSnapshot);
            });
        });
    }

    private void OnKicked()
    {
        if (_disposed) return;

        Dispatcher.UIThread.Invoke(() =>
        {
            UnsubscribeEvents();
            _dialog.ShowMessage(
                "You have been kicked.",
                "Information",
                DialogButton.OK, DialogIcon.Information);
            _navigation.GoBack();
        });
    }

    private void OnErrorReceived(string errorCode)
    {
        if (_disposed) return;

        Dispatcher.UIThread.Invoke(() =>
        {
            IsLoading = false;
            _dialog.ShowMessage(ErrorResolver.Resolve(errorCode),
                "Error",
                DialogButton.OK, DialogIcon.Error);
        });
    }

    // ── Commands ───────────────────────────────────────────────────────────

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
            // Silent — best-effort
        }
    }

    [RelayCommand]
    private async Task VoteToKickAsync(LobbyPlayerDto player)
    {
        if (player.IsHost) return;

        var result = _dialog.ShowMessage(
            $"Are you sure you want to vote to kick '{player.Username}'?",
            "Confirm",
            DialogButton.YesNo, DialogIcon.Question);

        if (result != DialogResult.Yes) return;

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
    private async Task LeaveAsync()
    {
        var result = _dialog.ShowMessage(
            "Are you sure you want to leave the lobby?",
            "Confirm",
            DialogButton.YesNo, DialogIcon.Question);

        if (result != DialogResult.Yes) return;

        try
        {
            await _lobbyService.LeaveLobbyAsync();
        }
        catch
        {
            // Best-effort
        }

        UnsubscribeEvents();
        _navigation.GoBack();
    }

    [RelayCommand]
    private void GoBack()
    {
        _ = SafeLeaveAsync();
        UnsubscribeEvents();
        _navigation.GoBack();
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private void AddSystemMessage(string message)
    {
        ChatMessages.Add($"⸻ {message} ⸻");
        ScrollChatToBottom?.Invoke();
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
