using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Localization;
using MemoryGame.Client.Models;
using MemoryGame.Client.Services.Core;
using MemoryGame.Client.Services.Interfaces;
using MemoryGame.Client.Services.Media;
using MemoryGame.Client.Services.Network;
using MemoryGame.Client.Services.UI;
using MemoryGame.Client.ViewModels.MainMenu;
using MemoryGame.Client.ViewModels.Common;

namespace MemoryGame.Client.ViewModels.Session;


/// <summary>
/// Handles user login via the REST API.
/// </summary>
public partial class LoginViewModel : BaseViewModel
{
    private readonly ApiClient _api;
    private readonly ISessionService _session;
    private readonly HubService _hub;

    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _password = string.Empty;


    public LoginViewModel(INavigationService navigation, IDialogService dialog, ApiClient api, ISessionService session, HubService hub)
        : base(navigation, dialog)
    {
        _api = api;
        _session = session;
        _hub = hub;
    }


    [RelayCommand]
    private Task LoginAsync() => RunAsync(async () =>
    {
        ErrorMessage = null;
        var result = await _api.PostAsync<LoginResponse>("api/auth/login", new { Username, Password });

        if (!result.IsSuccess)
        {
            ErrorMessage = ErrorResolver.Resolve(result.ErrorCode);
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

        Navigation.NavigateToRootWithFade<MainMenuViewModel>();
    });
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
