using System.IO;
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
using Microsoft.Win32;
using MemoryGame.Client.ViewModels.Common;

namespace MemoryGame.Client.ViewModels.Session;


/// <summary>
/// Lets the user pick a profile picture after email verification,
/// then finalizes registration and starts the session.
/// </summary>
public partial class SetupProfileViewModel : BaseViewModel
{
    private readonly ApiClient _api;
    private readonly ISessionService _session;
    private readonly HubService _hub;

    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _pin = string.Empty;
    [ObservableProperty] private byte[]? _avatarBytes;
    [ObservableProperty] private string? _avatarPreviewPath;


    public SetupProfileViewModel(
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
    private void SelectAvatar()
    {
        var dialog = new OpenFileDialog
        {
            Title = LocalizationManager.Instance["SetupProfile_Dialog_Title"],
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
            Multiselect = false
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                AvatarBytes = File.ReadAllBytes(dialog.FileName);
                AvatarPreviewPath = dialog.FileName;
            }
            catch
            {
                AvatarBytes = null;
                AvatarPreviewPath = null;
            }
        }
    }

    [RelayCommand]
    private Task FinalizeAsync() => RunAsync(async () =>
    {
        ErrorMessage = null;

        var result = await _api.PostAsync<FinalizeRegistrationResponse>(
            "api/auth/finalize-registration", new { Email, Pin });

        if (!result.IsSuccess)
        {
            ErrorMessage = result.ErrorMessage ?? "Registration failed.";
            return;
        }

        var data = result.Data!;

        _session.StartSession(new UserSession
        {
            UserId = data.User.Id,
            Username = data.User.Username,
            Email = data.User.Email,
            IsGuest = data.User.IsGuest,
            AccessToken = data.AccessToken,
            RefreshToken = data.RefreshToken
        });

        if (AvatarBytes is { Length: > 0 })
        {
            await _api.PutAsync("api/profile/avatar", new { AvatarData = AvatarBytes });
        }

        await _hub.ConnectAsync();

        Navigation.NavigateToRootWithFade<MainMenuViewModel>();
    });
}

