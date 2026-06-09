using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KatyaKatya.Models;
using KatyaKatya.Services.Interfaces;
using KatyaKatya.Services.Network;
using KatyaKatya.ViewModels.MainMenu;

namespace KatyaKatya.ViewModels.Session;

/// <summary>
/// Lets the user pick a profile picture after email verification,
/// then finalizes registration and starts the session.
/// </summary>
public partial class SetupProfileViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ApiClient _api;
    private readonly ISessionService _session;
    private readonly HubService _hub;

    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _pin = string.Empty;
    [ObservableProperty] private byte[]? _avatarBytes;
    [ObservableProperty] private string? _avatarPreviewPath;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isLoading;

    public SetupProfileViewModel(
        INavigationService navigation,
        ApiClient api,
        ISessionService session,
        HubService hub)
    {
        _navigation = navigation;
        _api = api;
        _session = session;
        _hub = hub;
    }

    [RelayCommand]
    private async Task SelectAvatarAsync()
    {
        // Platform file picker — requires TopLevel (injected at runtime from View code-behind
        // or via a platform-specific IFilePicker service, not added here yet).
        // For now, this is a no-op placeholder.
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task FinalizeAsync()
    {
        ErrorMessage = null;
        IsLoading = true;

        var result = await _api.PostAsync<FinalizeRegistrationResponse>(
            "api/auth/finalize-registration", new { Email, Pin });

        IsLoading = false;

        if (!result.IsSuccess)
        {
            ErrorMessage = result.ErrorMessage ?? "Registration failed.";
            return;
        }

        var data = result.Data!;

        _session.StartSession(new UserSession
        {
            UserId    = data.UserId,
            Username  = data.Username,
            Email     = data.Email,
            IsGuest   = data.IsGuest,
            AccessToken  = data.AccessToken,
            RefreshToken = data.RefreshToken
        });

        if (AvatarBytes is { Length: > 0 })
            await _api.PutAsync("api/profile/avatar", new { AvatarData = AvatarBytes });

        await _hub.ConnectAsync();
        _navigation.NavigateToRootWithFade<MainMenuViewModel>();
    }

    [RelayCommand]
    private void GoBack() => _navigation.GoBack();
}

public record FinalizeRegistrationResponse(
    int    UserId,
    string Username,
    string Email,
    bool   IsGuest,
    string AccessToken,
    string RefreshToken);
