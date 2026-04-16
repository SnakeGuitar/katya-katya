using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Helpers;
using MemoryGame.Client.Localization;
using MemoryGame.Client.Models;
using MemoryGame.Client.Services.Core;
using MemoryGame.Client.Services.Interfaces;
using MemoryGame.Client.Services.Media;
using MemoryGame.Client.Services.Network;
using MemoryGame.Client.Services.UI;
using MemoryGame.Client.ViewModels.Session;
using Microsoft.Win32;
using MemoryGame.Client.ViewModels.Common;

namespace MemoryGame.Client.ViewModels.Profile;

/// <summary>
/// Edit profile: avatar, personal info, social networks, username, password.
/// Mirrors the legacy EditProfile two-column layout.
/// </summary>
public partial class EditProfileViewModel : BaseViewModel
{
    private readonly ISessionService _session;
    private readonly HubService _hub;
    private readonly ProfileLoader _profileLoader;
    private readonly IProfileService _profileService;


    // Avatar
    [ObservableProperty] private byte[]? _avatarBytes;
    [ObservableProperty] private string? _avatarPreviewPath;

    // Personal info
    [ObservableProperty] private string _firstName = string.Empty;
    [ObservableProperty] private string _lastName = string.Empty;

    // Social networks
    [ObservableProperty] private string _newSocialAccount = string.Empty;
    public ObservableCollection<SocialNetworkDto> SocialNetworks { get; } = [];

    // Username
    [ObservableProperty] private string _newUsername = string.Empty;

    // Password
    [ObservableProperty] private string _currentPassword = string.Empty;
    [ObservableProperty] private string _newPassword = string.Empty;

    public EditProfileViewModel(

        INavigationService navigation,
        ISessionService session,
        IProfileService profileService,
        IDialogService dialog,
        HubService hub,
        ProfileLoader profileLoader) : base(navigation, dialog)
    {
        _session = session;
        _profileService = profileService;
        _hub = hub;
        _profileLoader = profileLoader;

        _ = LoadProfileDataAsync();
    }

    private Task LoadProfileDataAsync() => RunAsync(async () =>
    {
        await _profileLoader.LoadAllAsync();

        AvatarBytes = _profileLoader.Avatar;
        FirstName = _profileLoader.Name;
        LastName = _profileLoader.LastName;
        NewUsername = _profileLoader.Username;

        if (_profileLoader.SocialNetworks is not null)
        {
            SocialNetworks.Clear();
            foreach (var s in _profileLoader.SocialNetworks)
                SocialNetworks.Add(s);
        }
    });

    // ── Avatar ──────────────────────────────────────────────


    [RelayCommand]
    private async Task ChangeAvatarAsync()
    {
        var dialog = new OpenFileDialog
        {
            Title = LocalizationManager.Instance["EditProfile_Button_ChangeAvatar"],
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
            Multiselect = false
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            var bytes = await File.ReadAllBytesAsync(dialog.FileName);
            var result = await _profileService.UpdateAvatarAsync(bytes);

            if (await HandleResponseAsync(result, "EditProfile_Message_AvatarUpdated"))
            {
                AvatarBytes = bytes;
                AvatarPreviewPath = dialog.FileName;
            }
        }
        catch
        {
            Dialog.ShowMessage(LocalizationManager.Instance["Error_UNKNOWN"],
                LocalizationManager.Instance["Global_Title_Error"], DialogButton.OK, DialogIcon.Error);
        }

    }

    // ── Personal Info ───────────────────────────────────────

    [RelayCommand]
    private async Task UpdatePersonalInfoAsync()
    {
        var name = FirstName.Trim();
        var last = LastName.Trim();

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(last))
        {
            ShowWarning("Validation_Required");
            return;
        }

        await HandleResponseAsync(_profileService.UpdatePersonalInfoAsync(name, last), "EditProfile_Message_InfoUpdated");
    }

    // ── Social Networks ─────────────────────────────────────

    [RelayCommand]
    private async Task AddSocialAsync()
    {
        var account = NewSocialAccount.Trim();
        if (string.IsNullOrEmpty(account)) return;

        var result = await _profileService.AddSocialNetworkAsync(account);
        if (result is { IsSuccess: true, Data: not null })
        {
            SocialNetworks.Add(result.Data);
            NewSocialAccount = string.Empty;
        }
        else
        {
            await HandleResponseAsync(result);
        }
    }

    [RelayCommand]
    private async Task RemoveSocialAsync(int socialId)
    {
        if (await HandleResponseAsync(_profileService.RemoveSocialNetworkAsync(socialId)))
        {
            var item = SocialNetworks.FirstOrDefault(s => s.Id == socialId);
            if (item is not null)
                SocialNetworks.Remove(item);
        }
    }

    // ── Username ────────────────────────────────────────────

    [RelayCommand]
    private async Task UpdateUsernameAsync()
    {
        var username = NewUsername.Trim();
        if (string.IsNullOrWhiteSpace(username))
        {
            ShowWarning("Validation_Required");
            return;
        }

        if (username == _session.Current?.Username) return;

        if (await HandleResponseAsync(_profileService.UpdateUsernameAsync(username), "EditProfile_Message_UsernameUpdated"))
        {
            // Username change requires re-login
            await _hub.DisconnectAsync();
            _session.EndSession();
            Navigation.NavigateToRootWithFade<TitleScreenViewModel>();
        }
    }


    // ── Password ────────────────────────────────────────────

    [RelayCommand]
    private async Task UpdatePasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentPassword) || string.IsNullOrWhiteSpace(NewPassword))
        {
            ShowWarning("Validation_Required");
            return;
        }

        if (await HandleResponseAsync(_profileService.UpdatePasswordAsync(CurrentPassword, NewPassword), "EditProfile_Message_PasswordUpdated"))
        {
            ClearPasswords();
        }
    }

    // ── Utilities ───────────────────────────────────────────

    private void ClearPasswords()

    {
        CurrentPassword = string.Empty;
        NewPassword = string.Empty;
    }
}

