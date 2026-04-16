using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Helpers;
using MemoryGame.Client.Localization;
using MemoryGame.Client.Models;
using MemoryGame.Client.Services.Interfaces;
using MemoryGame.Client.Services.Network;
using MemoryGame.Client.ViewModels.Session;
using MemoryGame.Client.ViewModels.Social;

namespace MemoryGame.Client.ViewModels.Profile;

/// <summary>
/// Displays the authenticated user's profile with avatar, info, and navigation
/// to edit profile, friends, and statistics.
/// Mirrors the legacy PlayerProfile window layout.
/// </summary>
public partial class ProfileViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ISessionService _session;
    private readonly ApiClient _api;
    private readonly IDialogService _dialog;
    private readonly HubService _hub;
    private readonly ProfileLoader _profileLoader;

    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _fullName = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _registrationDate = string.Empty;
    [ObservableProperty] private byte[]? _avatarBytes;
    [ObservableProperty] private List<SocialNetworkDto> _socialNetworks = [];
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _hasSocialNetworks;

    public ProfileViewModel(
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

        _ = LoadProfileAsync();
    }

    private async Task LoadProfileAsync()
    {
        IsLoading = true;
        try
        {
            await _profileLoader.LoadAllAsync();

            Username = _profileLoader.Username;
            Email = _profileLoader.Email;
            AvatarBytes = _profileLoader.Avatar;
            RegistrationDate = _profileLoader.RegistrationDate.ToString("MMMM dd, yyyy");

            var name = $"{_profileLoader.Name} {_profileLoader.LastName}".Trim();
            FullName = string.IsNullOrEmpty(name)
                ? LocalizationManager.Instance["Profile_Label_NoInfo"]
                : name;

            if (_profileLoader.SocialNetworks is not null)
            {
                SocialNetworks = _profileLoader.SocialNetworks.ToList();
                HasSocialNetworks = SocialNetworks.Count > 0;
            }
        }
        finally
        {
            IsLoading = false;
        }
    }


    [RelayCommand]
    private void GoToEditProfile() => _navigation.NavigateTo<EditProfileViewModel>();

    [RelayCommand]
    private void GoToFriends() => _navigation.NavigateTo<FriendsViewModel>();

    [RelayCommand]
    private async Task LogoutAsync()
    {
        var result = _dialog.ShowMessage(
            LocalizationManager.Instance["Global_Button_Logout"] + "?",
            LocalizationManager.Instance["Global_Title_Confirm"],
            DialogButton.YesNo,
            DialogIcon.Question);

        if (result == DialogResult.Yes)
        {
            await _hub.DisconnectAsync();
            _session.EndSession();
            _navigation.NavigateToRootWithFade<TitleScreenViewModel>();
        }
    }

    [RelayCommand]
    private void GoBack() => _navigation.GoBack();
}
