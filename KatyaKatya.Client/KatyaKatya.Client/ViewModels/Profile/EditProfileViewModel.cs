using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KatyaKatya.Helpers;
using KatyaKatya.Models;
using KatyaKatya.Services.Interfaces;
using KatyaKatya.Services.Network;
using KatyaKatya.ViewModels.Session;

namespace KatyaKatya.ViewModels.Profile;

/// <summary>
/// Edit profile: avatar, personal info, social networks, username, password.
/// </summary>
public partial class EditProfileViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ISessionService _session;
    private readonly IDialogService _dialog;
    private readonly HubService _hub;
    private readonly ProfileLoader _profileLoader;
    private readonly IProfileService _profileService;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string? _errorMessage;

    // Avatar
    [ObservableProperty] private byte[]? _avatarBytes;

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
        ProfileLoader profileLoader)
    {
        _navigation = navigation;
        _session = session;
        _profileService = profileService;
        _dialog = dialog;
        _hub = hub;
        _profileLoader = profileLoader;

        _ = LoadProfileDataAsync();
    }

    private async Task LoadProfileDataAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
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
        }
        catch (Exception)
        {
            ErrorMessage = "Failed to load profile data.";
            _dialog.ShowMessage(ErrorMessage, "Error", DialogButton.OK, DialogIcon.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Avatar ──────────────────────────────────────────────

    public async Task UpdateAvatarDirectAsync(byte[] bytes)
    {
        IsLoading = true;
        try
        {
            var result = await _profileService.UpdateAvatarAsync(bytes);
            if (result.IsSuccess)
            {
                AvatarBytes = bytes;
                _dialog.ShowMessage("Avatar updated successfully!", "Success", DialogButton.OK, DialogIcon.Information);
            }
            else
            {
                var errMsg = ErrorResolver.Resolve(result.ErrorCode);
                _dialog.ShowMessage(errMsg, "Error", DialogButton.OK, DialogIcon.Error);
            }
        }
        catch (Exception ex)
        {
            _dialog.ShowMessage($"Failed to update avatar: {ex.Message}", "Error", DialogButton.OK, DialogIcon.Error);
        }
        finally
        {
            IsLoading = false;
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
            _dialog.ShowMessage("First Name and Last Name are required.", "Warning", DialogButton.OK, DialogIcon.Warning);
            return;
        }

        IsLoading = true;
        try
        {
            var result = await _profileService.UpdatePersonalInfoAsync(name, last);
            if (result.IsSuccess)
            {
                _dialog.ShowMessage("Personal info updated successfully!", "Success", DialogButton.OK, DialogIcon.Information);
            }
            else
            {
                var errMsg = ErrorResolver.Resolve(result.ErrorCode);
                _dialog.ShowMessage(errMsg, "Error", DialogButton.OK, DialogIcon.Error);
            }
        }
        catch (Exception ex)
        {
            _dialog.ShowMessage($"An error occurred: {ex.Message}", "Error", DialogButton.OK, DialogIcon.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Social Networks ─────────────────────────────────────

    [RelayCommand]
    private async Task AddSocialAsync()
    {
        var account = NewSocialAccount.Trim();
        if (string.IsNullOrEmpty(account)) return;

        IsLoading = true;
        try
        {
            var result = await _profileService.AddSocialNetworkAsync(account);
            if (result is { IsSuccess: true, Data: not null })
            {
                SocialNetworks.Add(result.Data);
                NewSocialAccount = string.Empty;
                _dialog.ShowMessage("Social account added successfully!", "Success", DialogButton.OK, DialogIcon.Information);
            }
            else
            {
                var errMsg = ErrorResolver.Resolve(result.ErrorCode);
                _dialog.ShowMessage(errMsg, "Error", DialogButton.OK, DialogIcon.Error);
            }
        }
        catch (Exception ex)
        {
            _dialog.ShowMessage($"An error occurred: {ex.Message}", "Error", DialogButton.OK, DialogIcon.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RemoveSocialAsync(int socialId)
    {
        IsLoading = true;
        try
        {
            var result = await _profileService.RemoveSocialNetworkAsync(socialId);
            if (result.IsSuccess)
            {
                var item = SocialNetworks.FirstOrDefault(s => s.Id == socialId);
                if (item is not null)
                {
                    SocialNetworks.Remove(item);
                }
                _dialog.ShowMessage("Social account removed successfully!", "Success", DialogButton.OK, DialogIcon.Information);
            }
            else
            {
                var errMsg = ErrorResolver.Resolve(result.ErrorCode);
                _dialog.ShowMessage(errMsg, "Error", DialogButton.OK, DialogIcon.Error);
            }
        }
        catch (Exception ex)
        {
            _dialog.ShowMessage($"An error occurred: {ex.Message}", "Error", DialogButton.OK, DialogIcon.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Username ────────────────────────────────────────────

    [RelayCommand]
    private async Task UpdateUsernameAsync()
    {
        var username = NewUsername.Trim();
        if (string.IsNullOrWhiteSpace(username))
        {
            _dialog.ShowMessage("Username is required.", "Warning", DialogButton.OK, DialogIcon.Warning);
            return;
        }

        if (username == _session.Current?.Username) return;

        IsLoading = true;
        try
        {
            var result = await _profileService.UpdateUsernameAsync(username);
            if (result.IsSuccess)
            {
                _dialog.ShowMessage("Username updated successfully! Please log in again.", "Success", DialogButton.OK, DialogIcon.Information);
                await _hub.DisconnectAsync();
                _session.EndSession();
                _navigation.NavigateToRootWithFade<TitleScreenViewModel>();
            }
            else
            {
                var errMsg = ErrorResolver.Resolve(result.ErrorCode);
                _dialog.ShowMessage(errMsg, "Error", DialogButton.OK, DialogIcon.Error);
            }
        }
        catch (Exception ex)
        {
            _dialog.ShowMessage($"An error occurred: {ex.Message}", "Error", DialogButton.OK, DialogIcon.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Password ────────────────────────────────────────────

    [RelayCommand]
    private async Task UpdatePasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentPassword) || string.IsNullOrWhiteSpace(NewPassword))
        {
            _dialog.ShowMessage("Current Password and New Password are required.", "Warning", DialogButton.OK, DialogIcon.Warning);
            return;
        }

        IsLoading = true;
        try
        {
            var result = await _profileService.UpdatePasswordAsync(CurrentPassword, NewPassword);
            if (result.IsSuccess)
            {
                _dialog.ShowMessage("Password updated successfully!", "Success", DialogButton.OK, DialogIcon.Information);
                CurrentPassword = string.Empty;
                NewPassword = string.Empty;
            }
            else
            {
                var errMsg = ErrorResolver.Resolve(result.ErrorCode);
                _dialog.ShowMessage(errMsg, "Error", DialogButton.OK, DialogIcon.Error);
            }
        }
        catch (Exception ex)
        {
            _dialog.ShowMessage($"An error occurred: {ex.Message}", "Error", DialogButton.OK, DialogIcon.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void GoBack() => _navigation.GoBack();
}
