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
/// Handles guest login — the user only needs to pick a username.
/// </summary>
public partial class GuestLoginViewModel : BaseViewModel
{
    private readonly ApiClient _api;
    private readonly ISessionService _session;
    private readonly HubService _hub;

    [ObservableProperty] private string _username = string.Empty;


    public GuestLoginViewModel(
        INavigationService navigation,
        IDialogService dialog,
        ApiClient api,
        ISessionService session,
        HubService hub) : base(navigation, dialog)
    {
        _api = api;
        _session = session;
        _hub = hub;
    }


    [RelayCommand]
    private Task LoginAsGuestAsync() => RunAsync(async () =>
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = ErrorResolver.Resolve("VALIDATION_USERNAME_EMPTY");
            return;
        }

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

        Navigation.NavigateToRootWithFade<MainMenuViewModel>();
    });
}

