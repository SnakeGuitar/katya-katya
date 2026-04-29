using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
/// Lobby menu — the entry point for multiplayer. Lets the player create a lobby
/// (with optional public flag) or join one by code / from the public list.
/// Public lobbies are fetched automatically on load and refreshed periodically via SignalR.
/// </summary>
public partial class LobbyMenuViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ISessionService _session;
    private readonly ILobbyService _lobbyService;
    private readonly IDialogService _dialog;
    private readonly HubService _hub;

    private readonly DispatcherTimer _refreshTimer;
    private bool _disposed;

    [ObservableProperty]
    private string _joinCode = string.Empty;

    [ObservableProperty]
    private bool _isPublic;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _joinCodeError;

    public ObservableCollection<LobbySummaryDto> PublicLobbies { get; } = new();

    public bool IsGuest => _session.Current?.IsGuest == true;

    public LobbyMenuViewModel(
        INavigationService navigation,
        ISessionService session,
        ILobbyService lobbyService,
        IDialogService dialog,
        HubService hub)
    {
        _navigation = navigation;
        _session = session;
        _lobbyService = lobbyService;
        _dialog = dialog;
        _hub = hub;

        _lobbyService.PublicLobbiesUpdated += OnPublicLobbiesReceived;

        // Periodic refresh every 5 seconds so the public lobby list stays current
        _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        _refreshTimer.Tick += async (_, _) => await RefreshPublicLobbiesAsync();
        _refreshTimer.Start();

        _ = LoadPublicLobbiesAsync();
    }

    private async Task LoadPublicLobbiesAsync()
    {
        try
        {
            await _hub.ConnectAsync();
            await _lobbyService.GetPublicLobbiesAsync();
        }
        catch
        {
            // Silent — public lobbies are a nice-to-have, not blocking
        }
    }

    private async Task RefreshPublicLobbiesAsync()
    {
        if (_disposed) return;
        try
        {
            await _lobbyService.GetPublicLobbiesAsync();
        }
        catch
        {
            // Silent
        }
    }

    [RelayCommand]
    private async Task CreateLobbyAsync()
    {
        if (IsGuest)
        {
            _dialog.ShowMessage(
                LocalizationManager.Instance["Menu_Error_RequiresFullAccount"],
                LocalizationManager.Instance["Global_Title_Warning"],
                DialogButton.OK, DialogIcon.Warning);
            return;
        }

        IsLoading = true;

        try
        {
            await _hub.ConnectAsync();

            string gameCode = GenerateGameCode();
            _lobbyService.LobbyCreated += OnLobbyCreated;
            _lobbyService.ErrorReceived += OnCreateError;
            await _lobbyService.CreateLobbyAsync(gameCode, IsPublic);
        }
        catch
        {
            IsLoading = false;
            _dialog.ShowMessage(
                LocalizationManager.Instance["Error_Network"],
                LocalizationManager.Instance["Global_Title_Error"],
                DialogButton.OK, DialogIcon.Error);
        }
    }

    private void OnLobbyCreated(string gameCode)
    {
        _lobbyService.LobbyCreated -= OnLobbyCreated;
        _lobbyService.ErrorReceived -= OnCreateError;
        IsLoading = false;

        App.Current.Dispatcher.Invoke(() =>
        {
            Cleanup();
            _navigation.NavigateTo<HostLobbyViewModel>(vm => vm.GameCode = gameCode);
        });
    }

    private void OnCreateError(string errorCode)
    {
        _lobbyService.LobbyCreated -= OnLobbyCreated;
        _lobbyService.ErrorReceived -= OnCreateError;
        IsLoading = false;

        App.Current.Dispatcher.Invoke(() =>
        {
            string message = LocalizationManager.Instance[$"Error_{errorCode}"]
                             ?? LocalizationManager.Instance["Error_Network"];
            _dialog.ShowMessage(message,
                LocalizationManager.Instance["Global_Title_Error"],
                DialogButton.OK, DialogIcon.Error);
        });
    }

    [RelayCommand]
    private async Task JoinByCodeAsync()
    {
        JoinCodeError = null;
        string code = JoinCode?.Trim() ?? string.Empty;

        if (code.Length != 6 || !int.TryParse(code, out _))
        {
            JoinCodeError = LocalizationManager.Instance["Error_InvalidGameCode"]
                            ?? "Enter a valid 6-digit code.";
            return;
        }

        IsLoading = true;

        try
        {
            await _hub.ConnectAsync();
            _lobbyService.ErrorReceived += OnJoinError;
            _lobbyService.PlayerListUpdated += OnJoinSuccess;
            await _lobbyService.JoinLobbyAsync(code);
        }
        catch
        {
            IsLoading = false;
            _dialog.ShowMessage(
                LocalizationManager.Instance["Error_Network"],
                LocalizationManager.Instance["Global_Title_Error"],
                DialogButton.OK, DialogIcon.Error);
        }
    }

    private void OnJoinSuccess(List<LobbyPlayerDto> _)
    {
        _lobbyService.PlayerListUpdated -= OnJoinSuccess;
        _lobbyService.ErrorReceived -= OnJoinError;
        IsLoading = false;

        App.Current.Dispatcher.Invoke(() =>
        {
            Cleanup();
            _navigation.NavigateTo<LobbyViewModel>(vm => vm.GameCode = JoinCode.Trim());
        });
    }

    private void OnJoinError(string errorCode)
    {
        _lobbyService.PlayerListUpdated -= OnJoinSuccess;
        _lobbyService.ErrorReceived -= OnJoinError;
        IsLoading = false;

        App.Current.Dispatcher.Invoke(() =>
        {
            string message = LocalizationManager.Instance[$"Error_{errorCode}"]
                             ?? LocalizationManager.Instance["Error_Network"];
            JoinCodeError = message;
        });
    }

    [RelayCommand]
    private async Task JoinPublicLobbyAsync(LobbySummaryDto lobby)
    {
        if (lobby.IsFull)
        {
            _dialog.ShowMessage(
                LocalizationManager.Instance["Error_LOBBY_FULL"]
                ?? "This lobby is full.",
                LocalizationManager.Instance["Global_Title_Information"],
                DialogButton.OK, DialogIcon.Information);
            return;
        }

        JoinCode = lobby.GameCode;
        await JoinByCodeAsync();
    }

    private void OnPublicLobbiesReceived(List<LobbySummaryDto> lobbies)
    {
        if (_disposed) return;

        App.Current.Dispatcher.Invoke(() =>
        {
            PublicLobbies.Clear();
            foreach (var lobby in lobbies)
                PublicLobbies.Add(lobby);
        });
    }

    [RelayCommand]
    private void GoBack()
    {
        Cleanup();
        _navigation.GoBack();
    }

    /// <summary>
    /// Stops the refresh timer and unsubscribes from events.
    /// Called when navigating away from this screen.
    /// </summary>
    private void Cleanup()
    {
        if (_disposed) return;
        _disposed = true;

        _refreshTimer.Stop();
        _lobbyService.PublicLobbiesUpdated -= OnPublicLobbiesReceived;
    }

    private static string GenerateGameCode()
        => Random.Shared.Next(100_000, 1_000_000).ToString();
}
