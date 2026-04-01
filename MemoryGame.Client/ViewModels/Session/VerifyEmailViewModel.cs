using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Models;
using MemoryGame.Client.Services;
using MemoryGame.Client.ViewModels.MainMenu;

namespace MemoryGame.Client.ViewModels.Session;

/// <summary>
/// Handles email verification PIN entry after registration.
/// After a successful PIN, the user is automatically logged in.
/// </summary>
public partial class VerifyEmailViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ApiClient          _api;
    private readonly ISessionService    _session;
    private readonly HubService         _hub;

    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _pin = string.Empty;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private string? _pinResentMessage;
    [ObservableProperty] private bool _isLoading;

    public VerifyEmailViewModel(
        INavigationService navigation,
        ApiClient          api,
        ISessionService    session,
        HubService         hub)
    {
        _navigation = navigation;
        _api        = api;
        _session    = session;
        _hub        = hub;
    }

    [RelayCommand]
    private async Task VerifyAsync()
    {
        ErrorMessage = null;
        IsLoading = true;

        try
        {
            var result = await _api.PostAsync<FinalizeRegistrationResponse>(
                "api/auth/finalize-registration", new { Email, Pin });

            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage ?? "Verification failed.";
                return;
            }

            var data = result.Data!;

            _session.StartSession(new UserSession
            {
                UserId       = data.User.Id,
                Username     = data.User.Username,
                Email        = data.User.Email,
                IsGuest      = data.User.IsGuest,
                AccessToken  = data.AccessToken,
                RefreshToken = data.RefreshToken
            });

            await _hub.ConnectAsync();

            _navigation.NavigateToRoot<MainMenuViewModel>();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ResendPinAsync()
    {
        PinResentMessage = null;
        var result = await _api.PostAsync("api/auth/resend-verification", new { Email });
        if (result.IsSuccess)
            PinResentMessage = Localization.LocalizationManager.Instance["VerifyEmail_PinResentMessage"];
    }

    [RelayCommand]
    private void GoBack() => _navigation.GoBack();
}

/// <summary>
/// It represents the response from the API when finalizing registration. 
/// It contains the access token, refresh token, and user information.
/// </summary>
public record FinalizeRegistrationResponse(
    string          AccessToken,
    string          RefreshToken,
    FinalizeUserDto User);

/// <summary>
/// It represents the user information returned from the API when finalizing registration.
/// </summary>
public record FinalizeUserDto(
    int    Id,
    string Username,
    string Email,
    bool   IsGuest,
    bool   VerifiedEmail);
