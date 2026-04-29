using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Localization;
using MemoryGame.Client.Models.Lobby;
using MemoryGame.Client.Services.Interfaces;
using MemoryGame.Client.Services.Network;

namespace MemoryGame.Client.ViewModels.Lobby;

/// <summary>
/// ViewModel for the player (non-host) lobby screen.
/// The player can see the player list, use the chat, and wait for the host to start.
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

    // ── Observable Properties ──────────────────────────────────────────────

    [ObservableProperty]
    private string _gameCode = string.Empty;

    [ObservableProperty]
    private string _chatMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public ObservableCollection<LobbyPlayerDto> Players { get; } = new();
    public ObservableCollection<string> ChatMessages { get; } = new();

    public bool IsGuest => _session.Current?.IsGuest == true;
    public string CurrentUsername => _session.Current?.Username ?? "Player";

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
        if (_isGameStarting) return;

        App.Current.Dispatcher.Invoke(() =>
        {
            Players.Clear();
            foreach (var player in players)
                Players.Add(player);
        });
    }

    private void OnPlayerJoined(string username, bool isGuest)
    {
        if (_isGameStarting) return;

        App.Current.Dispatcher.Invoke(() =>
        {
            string message = LocalizationManager.Instance.Format("Lobby_Notification_PlayerJoined", username);
            AddSystemMessage(message);
        });
    }

    private void OnPlayerLeft(string username)
    {
        if (_isGameStarting) return;

        App.Current.Dispatcher.Invoke(() =>
        {
            string message = LocalizationManager.Instance.Format("Lobby_Notification_PlayerLeft", username);
            AddSystemMessage(message);
        });
    }

    private void OnChatMessageReceived(string sender, string message, bool isSystem)
    {
        if (_isGameStarting) return;

        App.Current.Dispatcher.Invoke(() =>
        {
            string formatted = isSystem ? $"⸻ {message} ⸻" : $"{sender}: {message}";
            ChatMessages.Add(formatted);
        });
    }

    private void OnGameStarted(List<CardInfoDto> cards)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            _isGameStarting = true;
            UnsubscribeEvents();
            // TODO: Navigate to the multiplayer board view when implemented
        });
    }

    private void OnKicked()
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            UnsubscribeEvents();
            _dialog.ShowMessage(
                LocalizationManager.Instance["Lobby_Message_Kicked"],
                LocalizationManager.Instance["Global_Title_Information"],
                DialogButton.OK, DialogIcon.Information);
            _navigation.GoBack();
        });
    }

    private void OnErrorReceived(string errorCode)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            IsLoading = false;
            string message = LocalizationManager.Instance[$"Error_{errorCode}"]
                             ?? LocalizationManager.Instance["Error_UNKNOWN"];
            _dialog.ShowMessage(message,
                LocalizationManager.Instance["Global_Title_Error"],
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
            LocalizationManager.Instance.Format("KickVote_Message_VoteKickPlayer", player.Username),
            LocalizationManager.Instance["Global_Title_Confirm"],
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
    private async Task LeaveAsync()
    {
        var result = _dialog.ShowMessage(
            LocalizationManager.Instance["Lobby_Message_LeaveLobby"],
            LocalizationManager.Instance["Global_Title_Confirm"],
            DialogButton.YesNo, DialogIcon.Question);

        if (result != Services.Interfaces.DialogResult.Yes) return;

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
