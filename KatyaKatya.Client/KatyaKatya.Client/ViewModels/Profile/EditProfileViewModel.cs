using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KatyaKatya.Helpers;
using KatyaKatya.Localization;
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
    private readonly IFilePickerService _filePicker;

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
        ProfileLoader profileLoader,
        IFilePickerService filePicker)
    {
        _navigation = navigation;
        _session = session;
        _profileService = profileService;
        _dialog = dialog;
        _hub = hub;
        _profileLoader = profileLoader;
        _filePicker = filePicker;

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
            ErrorMessage = LocalizationManager.Instance["Error_UNKNOWN"];
            _dialog.ShowMessage(ErrorMessage, LocalizationManager.Instance["Global_Title_Error"], DialogButton.OK, DialogIcon.Error);
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
        var picked = await _filePicker.PickImageAsync();
        if (picked is null)
            return;

        await UpdateAvatarDirectAsync(picked.Bytes);
    }

    public async Task UpdateAvatarDirectAsync(byte[] bytes)
    {
        IsLoading = true;
        try
        {
            var result = await _profileService.UpdateAvatarAsync(bytes);
            if (result.IsSuccess)
            {
                AvatarBytes = bytes;
                _dialog.ShowMessage(LocalizationManager.Instance["EditProfile_Message_AvatarUpdated"],
                    LocalizationManager.Instance["Global_Title_Success"], DialogButton.OK, DialogIcon.Information);
            }
            else
            {
                var errMsg = ErrorResolver.Resolve(result.ErrorCode);
                _dialog.ShowMessage(errMsg, LocalizationManager.Instance["Global_Title_Error"], DialogButton.OK, DialogIcon.Error);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EditProfile] Avatar update failed: {ex.Message}");
            _dialog.ShowMessage(LocalizationManager.Instance["Error_UNKNOWN"],
                LocalizationManager.Instance["Global_Title_Error"], DialogButton.OK, DialogIcon.Error);
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
            _dialog.ShowMessage(LocalizationManager.Instance["Validation_Required"],
                LocalizationManager.Instance["Global_Title_Warning"], DialogButton.OK, DialogIcon.Warning);
            return;
        }

        IsLoading = true;
        try
        {
            var result = await _profileService.UpdatePersonalInfoAsync(name, last);
            if (result.IsSuccess)
            {
                _dialog.ShowMessage(LocalizationManager.Instance["EditProfile_Message_InfoUpdated"],
                    LocalizationManager.Instance["Global_Title_Success"], DialogButton.OK, DialogIcon.Information);
            }
            else
            {
                var errMsg = ErrorResolver.Resolve(result.ErrorCode);
                _dialog.ShowMessage(errMsg, LocalizationManager.Instance["Global_Title_Error"], DialogButton.OK, DialogIcon.Error);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EditProfile] Personal info update failed: {ex.Message}");
            _dialog.ShowMessage(LocalizationManager.Instance["Error_UNKNOWN"],
                LocalizationManager.Instance["Global_Title_Error"], DialogButton.OK, DialogIcon.Error);
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
                _dialog.ShowMessage(LocalizationManager.Instance["EditProfile_Message_SocialAdded"],
                    LocalizationManager.Instance["Global_Title_Success"], DialogButton.OK, DialogIcon.Information);
            }
            else
            {
                var errMsg = ErrorResolver.Resolve(result.ErrorCode);
                _dialog.ShowMessage(errMsg, LocalizationManager.Instance["Global_Title_Error"], DialogButton.OK, DialogIcon.Error);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EditProfile] Add social failed: {ex.Message}");
            _dialog.ShowMessage(LocalizationManager.Instance["Error_UNKNOWN"],
                LocalizationManager.Instance["Global_Title_Error"], DialogButton.OK, DialogIcon.Error);
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
                _dialog.ShowMessage(LocalizationManager.Instance["EditProfile_Message_SocialRemoved"],
                    LocalizationManager.Instance["Global_Title_Success"], DialogButton.OK, DialogIcon.Information);
            }
            else
            {
                var errMsg = ErrorResolver.Resolve(result.ErrorCode);
                _dialog.ShowMessage(errMsg, LocalizationManager.Instance["Global_Title_Error"], DialogButton.OK, DialogIcon.Error);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EditProfile] Remove social failed: {ex.Message}");
            _dialog.ShowMessage(LocalizationManager.Instance["Error_UNKNOWN"],
                LocalizationManager.Instance["Global_Title_Error"], DialogButton.OK, DialogIcon.Error);
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
            _dialog.ShowMessage(LocalizationManager.Instance["Validation_Required"],
                LocalizationManager.Instance["Global_Title_Warning"], DialogButton.OK, DialogIcon.Warning);
            return;
        }

        if (username == _session.Current?.Username) return;

        IsLoading = true;
        try
        {
            var result = await _profileService.UpdateUsernameAsync(username);
            if (result.IsSuccess)
            {
                _dialog.ShowMessage(LocalizationManager.Instance["EditProfile_Message_UsernameUpdated"],
                    LocalizationManager.Instance["Global_Title_Success"], DialogButton.OK, DialogIcon.Information);
                await _hub.DisconnectAsync();
                _session.EndSession();
                _navigation.NavigateToRootWithFade<TitleScreenViewModel>();
            }
            else
            {
                var errMsg = ErrorResolver.Resolve(result.ErrorCode);
                _dialog.ShowMessage(errMsg, LocalizationManager.Instance["Global_Title_Error"], DialogButton.OK, DialogIcon.Error);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EditProfile] Username update failed: {ex.Message}");
            _dialog.ShowMessage(LocalizationManager.Instance["Error_UNKNOWN"],
                LocalizationManager.Instance["Global_Title_Error"], DialogButton.OK, DialogIcon.Error);
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
            _dialog.ShowMessage(LocalizationManager.Instance["Validation_Required"],
                LocalizationManager.Instance["Global_Title_Warning"], DialogButton.OK, DialogIcon.Warning);
            return;
        }

        IsLoading = true;
        try
        {
            var result = await _profileService.UpdatePasswordAsync(CurrentPassword, NewPassword);
            if (result.IsSuccess)
            {
                _dialog.ShowMessage(LocalizationManager.Instance["EditProfile_Message_PasswordUpdated"],
                    LocalizationManager.Instance["Global_Title_Success"], DialogButton.OK, DialogIcon.Information);
                CurrentPassword = string.Empty;
                NewPassword = string.Empty;
            }
            else
            {
                var errMsg = ErrorResolver.Resolve(result.ErrorCode);
                _dialog.ShowMessage(errMsg, LocalizationManager.Instance["Global_Title_Error"], DialogButton.OK, DialogIcon.Error);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EditProfile] Password update failed: {ex.Message}");
            _dialog.ShowMessage(LocalizationManager.Instance["Error_UNKNOWN"],
                LocalizationManager.Instance["Global_Title_Error"], DialogButton.OK, DialogIcon.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void GoBack() => _navigation.GoBack();
}
