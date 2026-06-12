using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KatyaKatya.Helpers;
using KatyaKatya.Localization;
using KatyaKatya.Models.Lobby;
using KatyaKatya.Services.Core;
using KatyaKatya.Services.Interfaces;
using KatyaKatya.Services.Network;
using KatyaKatya.ViewModels.MainMenu;

namespace KatyaKatya.ViewModels.Lobby;

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
    private readonly ClientSettings _settings;

    private readonly DispatcherTimer _refreshTimer;
    private bool _disposed;

    [ObservableProperty]
    private string _joinCode = string.Empty;

    [ObservableProperty]
    private bool _isPublic;

    [ObservableProperty]
    private bool _isLoading;

    private bool _isJoining;

    [ObservableProperty]
    private string? _joinCodeError;

    [ObservableProperty]
    private bool _hasNoPublicLobbies = true;

    public ObservableCollection<LobbySummaryDto> PublicLobbies { get; } = new();

    public bool IsGuest => _session.Current?.IsGuest == true;
    public string BackgroundPath => ThemeAssets.GetGlobalBackgroundPath(_settings.ThemeName);

    public LobbyMenuViewModel(
        INavigationService navigation,
        ISessionService session,
        ILobbyService lobbyService,
        IDialogService dialog,
        HubService hub,
        ClientSettings settings)
    {
        _navigation = navigation;
        _session = session;
        _lobbyService = lobbyService;
        _dialog = dialog;
        _hub = hub;
        _settings = settings;

        _lobbyService.PublicLobbiesUpdated += OnPublicLobbiesReceived;
        _lobbyService.LobbyCreated += OnLobbyCreated;
        _lobbyService.ErrorReceived += OnLobbyError;
        _lobbyService.PlayerListUpdated += OnJoinSuccess;

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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LobbyMenu] Initial connection failed: {ex.Message}");
        }
    }

    private async Task RefreshPublicLobbiesAsync()
    {
        if (_disposed || IsLoading || !_hub.IsConnected) return;
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

        _isJoining = false;
        IsLoading = true;

        try
        {
            // Set a timeout for the connection and creation process
            var connectTask = _hub.ConnectAsync();
            var timeoutTask = Task.Delay(10000); // 10 seconds timeout

            var completedTask = await Task.WhenAny(connectTask, timeoutTask);
            if (completedTask == timeoutTask)
            {
                throw new TimeoutException("Connection to server timed out.");
            }

            await connectTask; // Propagate any exception from the connection itself

            string gameCode = GenerateGameCode();
            var createLobbyTask = _lobbyService.CreateLobbyAsync(gameCode, IsPublic);
            var createTimeoutTask = Task.Delay(10000);
            var completedCreateTask = await Task.WhenAny(createLobbyTask, createTimeoutTask);

            if (completedCreateTask == createTimeoutTask)
            {
                throw new TimeoutException("Server did not respond to CreateLobby.");
            }

            await createLobbyTask;
        }
        catch (Exception ex)
        {
            IsLoading = false;
            string errorMessage = ex is TimeoutException 
                ? LocalizationManager.Instance.TryGet("Error_ConnectionTimeout") ?? "Timed out connecting to server."
                : $"{LocalizationManager.Instance["Global_Title_Error"]}: {ex.Message}";

            _dialog.ShowMessage(errorMessage,
                LocalizationManager.Instance["Global_Title_Error"],
                DialogButton.OK, DialogIcon.Error);
        }
    }

    private void OnLobbyCreated(string gameCode)
    {
        if (_disposed) return;

        Dispatcher.UIThread.Invoke(() =>
        {
            IsLoading = false;
            JoinCode = string.Empty; // Clear fields for the next time
            JoinCodeError = null;
            IsPublic = false;
            _navigation.NavigateTo<HostLobbyViewModel>(vm => vm.GameCode = gameCode);
        });
    }

    private void OnLobbyError(string errorCode)
    {
        if (_disposed) return;

        Dispatcher.UIThread.Invoke(() =>
        {
            IsLoading = false;
            _isJoining = false;

            if (errorCode == "LOBBY_NOT_FOUND" || errorCode == "LOBBY_FULL" || errorCode == "LOBBY_GAME_IN_PROGRESS")
            {
                JoinCodeError = ErrorResolver.Resolve(errorCode);
            }
            else
            {
                _dialog.ShowMessage(ErrorResolver.Resolve(errorCode),
                    LocalizationManager.Instance["Global_Title_Error"],
                    DialogButton.OK, DialogIcon.Error);
            }
        });
    }

    [RelayCommand]
    private async Task JoinByCodeAsync()
    {
        JoinCodeError = null;
        string code = JoinCode?.Trim() ?? string.Empty;

        if (code.Length != 6 || !int.TryParse(code, out _))
        {
            JoinCodeError = LocalizationManager.Instance["Error_InvalidGameCode"];
            return;
        }

        _isJoining = true;
        IsLoading = true;

        try
        {
            await _hub.ConnectAsync();
            await _lobbyService.JoinLobbyAsync(code);
        }
        catch (Exception ex)
        {
            IsLoading = false;
            _isJoining = false;
            _dialog.ShowMessage($"Join failed: {ex.Message}",
                LocalizationManager.Instance["Global_Title_Error"],
                DialogButton.OK, DialogIcon.Error);
        }
    }

    private void OnJoinSuccess(List<LobbyPlayerDto> _)
    {
        if (_disposed || !_isJoining) return;

        Dispatcher.UIThread.Invoke(() =>
        {
            IsLoading = false;
            string code = JoinCode?.Trim() ?? string.Empty;
            JoinCode = string.Empty; // Clear fields for the next time
            JoinCodeError = null;

            // Unsubscribe to avoid multiple navigations if more updates arrive
            _lobbyService.PlayerListUpdated -= OnJoinSuccess;

            _navigation.NavigateTo<LobbyViewModel>(vm => vm.GameCode = code);
        });
    }

    [RelayCommand]
    private async Task JoinPublicLobbyAsync(LobbySummaryDto lobby)
    {
        if (lobby.IsFull)
        {
            _dialog.ShowMessage(
                LocalizationManager.Instance["Error_LOBBY_FULL"],
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

        Dispatcher.UIThread.Invoke(() =>
        {
            PublicLobbies.Clear();
            foreach (var lobby in lobbies)
                PublicLobbies.Add(lobby);

            HasNoPublicLobbies = PublicLobbies.Count == 0;
        });
    }

    [RelayCommand]
    private void GoBack()
    {
        Cleanup();
        
        if (_navigation.CanGoBack)
        {
            _navigation.GoBack();
        }
        else
        {
            _navigation.NavigateToRootWithFade<MainMenuViewModel>();
        }
    }

    private void Cleanup()
    {
        if (_disposed) return;
        _disposed = true;

        _refreshTimer.Stop();
        _lobbyService.PublicLobbiesUpdated -= OnPublicLobbiesReceived;
        _lobbyService.LobbyCreated -= OnLobbyCreated;
        _lobbyService.ErrorReceived -= OnLobbyError;
        _lobbyService.PlayerListUpdated -= OnJoinSuccess;
    }

    private static string GenerateGameCode()
        => Random.Shared.Next(100_000, 1_000_000).ToString();
}
