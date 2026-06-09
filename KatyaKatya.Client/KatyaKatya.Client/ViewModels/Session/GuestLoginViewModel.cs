using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KatyaKatya.Models;
using KatyaKatya.Services.Interfaces;
using KatyaKatya.Services.Network;
using KatyaKatya.Services.Core;
using KatyaKatya.Helpers;
using KatyaKatya.ViewModels.MainMenu;

namespace KatyaKatya.ViewModels.Session;

public partial class GuestLoginViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ISessionService _session;
    private readonly ApiClient _api;
    private readonly HubService _hub;
    private readonly ClientSettings _settings;

    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isLoading;

    public string LogoPath => LogoResolver.Resolve(_settings.LanguageCode, _settings.ThemeName);

    public GuestLoginViewModel(INavigationService navigation, ISessionService session,
        ApiClient api, HubService hub, ClientSettings settings)
    {
        _navigation = navigation;
        _session = session;
        _api = api;
        _hub = hub;
        _settings = settings;
    }

    [RelayCommand]
    private async Task LoginAsGuestAsync()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Please enter a username.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        var result = await _api.PostAsync<LoginResponse>(
            "api/auth/login-guest", new { GuestUsername = Username });

        IsLoading = false;

        if (result.IsSuccess && result.Data is not null)
        {
            _session.StartSession(new UserSession
            {
                UserId = result.Data.UserId,
                Username = result.Data.Username,
                Email = result.Data.Email,
                IsGuest = true,
                AccessToken = result.Data.AccessToken,
                RefreshToken = result.Data.RefreshToken
            });

            await _hub.ConnectAsync();
            _navigation.NavigateToRootWithFade<MainMenuViewModel>();
        }
        else
        {
            ErrorMessage = result.ErrorMessage ?? "Guest login failed.";
        }
    }

    [RelayCommand]
    private void GoBack() => _navigation.GoBack();
}
