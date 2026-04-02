using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Localization;
using MemoryGame.Client.Models;
using MemoryGame.Client.Services;
using MemoryGame.Client.ViewModels.MainMenu;

namespace MemoryGame.Client.ViewModels.Session;

/// <summary>
/// Handles guest login — the user only needs to pick a username.
/// </summary>
public partial class GuestLoginViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ApiClient _api;
    private readonly ISessionService _session;
    private readonly HubService _hub;

    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isLoading;

    public GuestLoginViewModel(
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
    private async Task LoginAsGuestAsync()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = ErrorResolver.Resolve("VALIDATION_USERNAME_EMPTY");
            return;
        }

        IsLoading = true;

        try
        {
            var result = await _api.PostAsync<LoginResponse>(
                "api/auth/login-guest", new { GuestUsername = Username });

            if (!result.IsSuccess)
            {
                ErrorMessage = ErrorResolver.Resolve(result.ErrorCode);
                return;
            }

            var data = result.Data!;

            _session.StartSession(new UserSession
            {
                UserId = data.UserId,
                Username = data.Username,
                Email = data.Email,
                IsGuest = true,
                AccessToken = data.AccessToken,
                RefreshToken = data.RefreshToken
            });

            await _hub.ConnectAsync();

            _navigation.NavigateToRootWithFade<MainMenuViewModel>();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void GoBack() => _navigation.GoBack();
}
