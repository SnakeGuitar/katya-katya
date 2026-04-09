using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Services.Core;
using MemoryGame.Client.Services.Interfaces;
using MemoryGame.Client.Services.Media;
using MemoryGame.Client.Services.Network;
using MemoryGame.Client.Services.UI;
using MemoryGame.Client.ViewModels.Settings;

using System.Windows;
using MemoryGame.Client.Localization;
using MemoryGame.Client.ViewModels.Session;
using MemoryGame.Client.ViewModels.Profile;

namespace MemoryGame.Client.ViewModels;

/// <summary>
/// Shell view model. Holds the current view via the navigation service
/// and exposes window-level commands (e.g. F11 fullscreen toggle).
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly IWindowService     _window;
    private readonly ISessionService    _session;
    private readonly IDialogService     _dialogs;

    public MainWindowViewModel(INavigationService navigation, IWindowService window, ISessionService session, IDialogService dialogs)
    {
        _navigation = navigation;
        _window     = window;
        _session    = session;
        _dialogs    = dialogs;

        if (_navigation is System.ComponentModel.INotifyPropertyChanged npc)
        {
            npc.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(INavigationService.CurrentViewModel))
                {
                    OnPropertyChanged(nameof(GuestRegisterVisibility));
                }
            };
        }
    }

    /// <summary>The navigation service that exposes the current view model to the shell.</summary>
    public INavigationService Navigation => _navigation;

    public Visibility GuestRegisterVisibility => 
        _session.IsLoggedIn && _session.Current?.IsGuest == true ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>Bound to the global settings button in MainWindow.xaml.</summary>
    [RelayCommand]
    private void GoToSettings() => _navigation.NavigateTo<SettingsViewModel>();

    /// <summary>Bound to the global profile button in MainWindow.xaml.</summary>
    [RelayCommand]
    private void GoToProfile() 
    {
        if (!CheckCanAccessProtectedSection()) return;
        _navigation.NavigateTo<ProfileViewModel>();
    }

    /// <summary>Bound to the global friends button in MainWindow.xaml dropdown.</summary>
    [RelayCommand]
    private void GoToFriends()
    {
        if (!CheckCanAccessProtectedSection()) return;
        _navigation.NavigateTo<Social.FriendsViewModel>();
    }

    [RelayCommand]
    private void GoToGuestRegister()
    {
        if (_session.IsLoggedIn && _session.Current?.IsGuest == true)
        {
            _navigation.NavigateTo<RegisterViewModel>();
        }
    }

    /// <summary>Bound to the F11 KeyBinding in MainWindow.xaml.</summary>
    [RelayCommand]
    private void ToggleFullscreen() => _window.ToggleFullscreen();

    private bool CheckCanAccessProtectedSection()
    {
        if (IsSessionFlow() && _session.IsLoggedIn)
        {
            return false;
        }

        if (!IsLoggedIn()) return false;
        
        if (IsLoggedInAsGuest()) return false;

        return true;
    }

    private bool IsSessionFlow()
    {
        var vm = _navigation.CurrentViewModel;
        return vm is TitleScreenViewModel ||
               vm is LoginViewModel ||
               vm is RegisterViewModel ||
               vm is GuestLoginViewModel ||
               vm is VerifyEmailViewModel ||
               vm is SetupProfileViewModel;
    }

    private bool IsLoggedIn()
    {
        if (!_session.IsLoggedIn)
        {
            _dialogs.ShowMessage(LocalizationManager.Instance["Menu_Error_RequiresLogin"], icon: DialogIcon.Warning);
            return false;
        }
        return true;
    }

    private bool IsLoggedInAsGuest()
    {
        if (_session.IsLoggedIn && _session.Current?.IsGuest == true)
        {
            _dialogs.ShowMessage(LocalizationManager.Instance["Menu_Error_RequiresFullAccount"], icon: DialogIcon.Warning);
            return true;
        }
        return false;
    }
}