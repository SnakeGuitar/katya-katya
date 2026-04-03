using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Localization;
using MemoryGame.Client.Models;
using MemoryGame.Client.Services;
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
        HubService hub)
    {
        _navigation = navigation;
        _session = session;
        _api = api;
        _dialog = dialog;
        _hub = hub;

        _ = LoadProfileAsync();
    }

    private async Task LoadProfileAsync()
    {
        IsLoading = true;
        try
        {
            var result = await _api.GetAsync<ProfileResponse>("api/profile");
            if (result is { IsSuccess: true, Data: not null })
            {
                var profile = result.Data;
                Username = profile.Username;
                Email = profile.Email;
                AvatarBytes = profile.Avatar;
                RegistrationDate = profile.RegistrationDate.ToString("MMMM dd, yyyy");

                var name = $"{profile.Name} {profile.LastName}".Trim();
                FullName = string.IsNullOrEmpty(name)
                    ? LocalizationManager.Instance["Profile_Label_NoInfo"]
                    : name;

                // Load social networks
                var socialsResult = await _api.GetAsync<SocialNetworkDto[]>("api/social/networks");
                if (socialsResult is { IsSuccess: true, Data: not null })
                {
                    SocialNetworks = socialsResult.Data.ToList();
                    HasSocialNetworks = SocialNetworks.Count > 0;
                }
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

        if (result == Services.DialogResult.Yes)
        {
            await _hub.DisconnectAsync();
            _session.EndSession();
            _navigation.NavigateToRootWithFade<TitleScreenViewModel>();
        }
    }

    [RelayCommand]
    private void GoBack() => _navigation.GoBack();
}
