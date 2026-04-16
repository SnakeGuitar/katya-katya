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

namespace MemoryGame.Client.ViewModels.Profile;

/// <summary>
/// Edit profile: avatar, personal info, social networks, username, password.
/// Mirrors the legacy EditProfile two-column layout.
/// </summary>
public partial class EditProfileViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ISessionService _session;
    private readonly ApiClient _api;
    private readonly IDialogService _dialog;
    private readonly HubService _hub;
    private readonly ProfileLoader _profileLoader;

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

    // State
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isLoading;

    public EditProfileViewModel(
        INavigationService navigation,
        ISessionService session,
        ApiClient api,
        IDialogService dialog,
        HubService hub,
        ProfileLoader profileLoader)
    {
        _navigation = navigation;
        _session = session;
        _api = api;
        _dialog = dialog;
        _hub = hub;
        _profileLoader = profileLoader;

        _ = LoadProfileDataAsync();
    }

    private async Task LoadProfileDataAsync()
    {
        IsLoading = true;
        try
        {
            await _profileLoader.LoadAllAsync();

            if (_profileLoader.Profile is not null)
            {
                var p = _profileLoader.Profile;
                AvatarBytes = p.Avatar;
                FirstName = p.Name ?? string.Empty;
                LastName = p.LastName ?? string.Empty;
                NewUsername = p.Username;
            }

            if (_profileLoader.SocialNetworks is not null)
            {
                SocialNetworks.Clear();
                foreach (var s in _profileLoader.SocialNetworks)
                    SocialNetworks.Add(s);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

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

            var result = await _api.PutAsync("api/profile/avatar", new { AvatarData = bytes });
            if (result.IsSuccess)
            {
                AvatarBytes = bytes;
                AvatarPreviewPath = dialog.FileName;
                _dialog.ShowMessage(
                    LocalizationManager.Instance["EditProfile_Message_AvatarUpdated"],
                    LocalizationManager.Instance["Global_Title_Success"],
                    DialogButton.OK, DialogIcon.Information);
            }
            else
            {
                _dialog.ShowMessage(
                    ErrorResolver.Resolve(result.ErrorCode),
                    LocalizationManager.Instance["Global_Title_Error"],
                    DialogButton.OK, DialogIcon.Error);
            }
        }
        catch
        {
            _dialog.ShowMessage(
                LocalizationManager.Instance["Error_UNKNOWN"],
                LocalizationManager.Instance["Global_Title_Error"],
                DialogButton.OK, DialogIcon.Error);
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
            _dialog.ShowMessage(
                LocalizationManager.Instance["Validation_Required"],
                LocalizationManager.Instance["Global_Title_Warning"],
                DialogButton.OK, DialogIcon.Warning);
            return;
        }

        var result = await _api.PutAsync("api/profile/personal-info", new { Name = name, LastName = last });
        if (result.IsSuccess)
        {
            _dialog.ShowMessage(
                LocalizationManager.Instance["EditProfile_Message_InfoUpdated"],
                LocalizationManager.Instance["Global_Title_Success"],
                DialogButton.OK, DialogIcon.Information);
        }
        else
        {
            _dialog.ShowMessage(
                ErrorResolver.Resolve(result.ErrorCode),
                LocalizationManager.Instance["Global_Title_Error"],
                DialogButton.OK, DialogIcon.Error);
        }
    }

    // ── Social Networks ─────────────────────────────────────

    [RelayCommand]
    private async Task AddSocialAsync()
    {
        var account = NewSocialAccount.Trim();
        if (string.IsNullOrEmpty(account)) return;

        var result = await _api.PostAsync<SocialNetworkDto>("api/social/networks", new { Account = account });
        if (result is { IsSuccess: true, Data: not null })
        {
            SocialNetworks.Add(result.Data);
            NewSocialAccount = string.Empty;
        }
        else
        {
            _dialog.ShowMessage(
                ErrorResolver.Resolve(result.ErrorCode),
                LocalizationManager.Instance["Global_Title_Error"],
                DialogButton.OK, DialogIcon.Error);
        }
    }

    [RelayCommand]
    private async Task RemoveSocialAsync(int socialId)
    {
        var result = await _api.DeleteAsync($"api/social/networks/{socialId}");
        if (result.IsSuccess)
        {
            var item = SocialNetworks.FirstOrDefault(s => s.Id == socialId);
            if (item is not null)
                SocialNetworks.Remove(item);
        }
        else
        {
            _dialog.ShowMessage(
                ErrorResolver.Resolve(result.ErrorCode),
                LocalizationManager.Instance["Global_Title_Error"],
                DialogButton.OK, DialogIcon.Error);
        }
    }

    // ── Username ────────────────────────────────────────────

    [RelayCommand]
    private async Task UpdateUsernameAsync()
    {
        var username = NewUsername.Trim();
        if (string.IsNullOrWhiteSpace(username))
        {
            _dialog.ShowMessage(
                LocalizationManager.Instance["Validation_Required"],
                LocalizationManager.Instance["Global_Title_Warning"],
                DialogButton.OK, DialogIcon.Warning);
            return;
        }

        if (username == _session.Current?.Username)
            return;

        var result = await _api.PutAsync("api/profile/username", new { NewUsername = username });
        if (result.IsSuccess)
        {
            _dialog.ShowMessage(
                LocalizationManager.Instance["EditProfile_Message_UsernameUpdated"],
                LocalizationManager.Instance["Global_Title_Success"],
                DialogButton.OK, DialogIcon.Information);

            // Username change requires re-login
            await _hub.DisconnectAsync();
            _session.EndSession();
            _navigation.NavigateToRootWithFade<TitleScreenViewModel>();
        }
        else
        {
            _dialog.ShowMessage(
                ErrorResolver.Resolve(result.ErrorCode),
                LocalizationManager.Instance["Global_Title_Error"],
                DialogButton.OK, DialogIcon.Error);
        }
    }

    // ── Password ────────────────────────────────────────────

    [RelayCommand]
    private async Task UpdatePasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentPassword) || string.IsNullOrWhiteSpace(NewPassword))
        {
            _dialog.ShowMessage(
                LocalizationManager.Instance["Validation_Required"],
                LocalizationManager.Instance["Global_Title_Warning"],
                DialogButton.OK, DialogIcon.Warning);
            return;
        }

        var result = await _api.PutAsync("api/profile/password",
            new { CurrentPassword, NewPassword });

        if (result.IsSuccess)
        {
            _dialog.ShowMessage(
                LocalizationManager.Instance["EditProfile_Message_PasswordUpdated"],
                LocalizationManager.Instance["Global_Title_Success"],
                DialogButton.OK, DialogIcon.Information);
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
        }
        else
        {
            _dialog.ShowMessage(
                ErrorResolver.Resolve(result.ErrorCode),
                LocalizationManager.Instance["Global_Title_Error"],
                DialogButton.OK, DialogIcon.Error);
        }
    }

    // ── Navigation ──────────────────────────────────────────

    [RelayCommand]
    private void GoBack() => _navigation.GoBack();
}
