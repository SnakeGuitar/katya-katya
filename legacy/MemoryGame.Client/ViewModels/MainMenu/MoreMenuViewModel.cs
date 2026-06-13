using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Localization;
using MemoryGame.Client.Services.Core;
using MemoryGame.Client.Services.Interfaces;
using MemoryGame.Client.Services.Media;
using MemoryGame.Client.Services.Network;
using MemoryGame.Client.Services.UI;
using MemoryGame.Client.ViewModels.Gallery;
using MemoryGame.Client.ViewModels.Profile;
using MemoryGame.Client.ViewModels.Social;

namespace MemoryGame.Client.ViewModels.MainMenu;

/// <summary>
/// Secondary "More" menu hub. Provides navigation to Gallery, Profile and Friends.
/// Profile and Friends are gated — guest accounts cannot access them.
/// </summary>
public partial class MoreMenuViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ISessionService _session;
    private readonly IDialogService _dialog;

    public MoreMenuViewModel(
        INavigationService navigation,
        ISessionService session,
        IDialogService dialog)
    {
        _navigation = navigation;
        _session = session;
        _dialog = dialog;
    }

    [RelayCommand]
    private void GoToGallery() => _navigation.NavigateTo<GalleryViewModel>();

    [RelayCommand]
    private void GoToProfile()
    {
        if (!RequireFullAccount()) return;
        _navigation.NavigateTo<ProfileViewModel>();
    }

    [RelayCommand]
    private void GoToFriends()
    {
        if (!RequireFullAccount()) return;
        _navigation.NavigateTo<FriendsViewModel>();
    }

    [RelayCommand]
    private void GoBack() => _navigation.GoBack();

    /// <summary>
    /// Returns true when the current user has a registered (non-guest) account.
    /// Shows an error dialog and returns false for guests.
    /// </summary>
    private bool RequireFullAccount()
    {
        if (_session.Current?.IsGuest != true) return true;

        _dialog.ShowMessage(
            LocalizationManager.Instance["Menu_Error_RequiresFullAccount"],
            LocalizationManager.Instance["Global_Title_Warning"],
            DialogButton.OK,
            DialogIcon.Warning);

        return false;
    }
}
