using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KatyaKatya.Services.Interfaces;
using KatyaKatya.ViewModels.Session;
using KatyaKatya.ViewModels.Settings;
using KatyaKatya.ViewModels.Profile;
using KatyaKatya.ViewModels.Social;

namespace KatyaKatya.ViewModels;

/// <summary>
/// Shell view model. Holds the current view via the navigation service
/// and exposes window-level commands.
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly IWindowService _window;
    private readonly ISessionService _session;
    private readonly IDialogService _dialogs;

    public MainWindowViewModel(INavigationService navigation, IWindowService window,
        ISessionService session, IDialogService dialogs)
    {
        _navigation = navigation;
        _window = window;
        _session = session;
        _dialogs = dialogs;

        if (_navigation is System.ComponentModel.INotifyPropertyChanged npc)
        {
            npc.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(INavigationService.CurrentViewModel))
                {
                    OnPropertyChanged(nameof(IsHeaderVisible));
                    OnPropertyChanged(nameof(IsGuestRegisterVisible));
                }
            };
        }
    }

    public INavigationService Navigation => _navigation;

    public bool IsGuestRegisterVisible =>
        _session.IsLoggedIn && _session.Current?.IsGuest == true;

    public bool IsHeaderVisible
    {
        get
        {
            var vm = _navigation.CurrentViewModel;
            return vm is not (TitleScreenViewModel or LoginViewModel or RegisterViewModel
                              or GuestLoginViewModel or SplashScreenViewModel);
        }
    }

    [RelayCommand]
    private void GoToSettings() => _navigation.NavigateTo<SettingsViewModel>();

    [RelayCommand]
    private void GoToProfile()
    {
        if (!CheckCanAccessProtectedSection()) return;
        _navigation.NavigateTo<ProfileViewModel>();
    }

    [RelayCommand]
    private void GoToFriends()
    {
        if (!CheckCanAccessProtectedSection()) return;
        _navigation.NavigateTo<FriendsViewModel>();
    }

    [RelayCommand]
    private void GoToGuestRegister()
    {
        if (_session.IsLoggedIn && _session.Current?.IsGuest == true)
            _navigation.NavigateTo<RegisterViewModel>();
    }

    [RelayCommand]
    private void ToggleFullscreen() => _window.ToggleFullscreen();

    private bool CheckCanAccessProtectedSection()
    {
        if (!_session.IsLoggedIn)
        {
            _dialogs.ShowMessage("You must be logged in to access this section.", icon: DialogIcon.Warning);
            return false;
        }

        if (_session.Current?.IsGuest == true)
        {
            _dialogs.ShowMessage("This feature requires a full account.", icon: DialogIcon.Warning);
            return false;
        }

        return true;
    }
}
