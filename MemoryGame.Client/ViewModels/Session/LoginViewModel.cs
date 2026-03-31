using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Models;
using MemoryGame.Client.Services;
using MemoryGame.Client.ViewModels.MainMenu;

namespace MemoryGame.Client.ViewModels.Session;

/// <summary>
/// Handles user login via the REST API.
/// </summary>
public partial class LoginViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ApiClient _api;
    private readonly ISessionService _session;
    private readonly HubService _hub;

    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isLoading;

    public LoginViewModel(INavigationService navigation, ApiClient api, ISessionService session, HubService hub)
    {
        _navigation = navigation;
        _api = api;
        _session = session;
        _hub = hub;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = null;
        IsLoading = true;

        try
        {
            var result = await _api.PostAsync<LoginResponse>("api/auth/login", new { Username, Password });

            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage ?? "Login failed.";
                return;
            }

            _session.StartSession(new UserSession
            {
                UserId = result.Data!.UserId,
                Username = result.Data.Username,
                Email = result.Data.Email,
                IsGuest = result.Data.IsGuest,
                AccessToken = result.Data.AccessToken,
                RefreshToken = result.Data.RefreshToken
            });

            await _hub.ConnectAsync();

            _navigation.NavigateTo<MainMenuViewModel>();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigation.NavigateTo<TitleScreenViewModel>();
    }
}

/// <summary>
/// DTO matching the server's login response.
/// </summary>
public record LoginResponse(
    int UserId,
    string Username,
    string Email,
    bool IsGuest,
    string AccessToken,
    string RefreshToken);
