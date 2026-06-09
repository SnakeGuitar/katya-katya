using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KatyaKatya.Services.Interfaces;
using KatyaKatya.ViewModels.Session;
using KatyaKatya.ViewModels.Settings;
using KatyaKatya.ViewModels.Profile;
using KatyaKatya.ViewModels.Social;
using KatyaKatya.Localization;
using KatyaKatya.ViewModels.Lobby;

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

    [ObservableProperty]
    private ObservableObject? _displayedViewModel;

    public event Action<ObservableObject?>? AnimatedNavigationRequested;

    public MainWindowViewModel(INavigationService navigation, IWindowService window,
        ISessionService session, IDialogService dialogs)
    {
        _navigation = navigation;
        _window = window;
        _session = session;
        _dialogs = dialogs;

        if (_navigation is System.ComponentModel.INotifyPropertyChanged npc)
        {
            npc.PropertyChanged += OnNavigationPropertyChanged;
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
                              or GuestLoginViewModel or SplashScreenViewModel
                              or VerifyEmailViewModel or SetupProfileViewModel);
        }
    }

    [RelayCommand]
    private void GoToSettings()
    {
        CloseGameBoardModalIfOpen();
        _navigation.NavigateTo<SettingsViewModel>();
    }

    [RelayCommand]
    private void GoToProfile()
    {
        if (!CheckCanAccessProtectedSection()) return;
        CloseGameBoardModalIfOpen();
        _navigation.NavigateTo<ProfileViewModel>();
    }

    [RelayCommand]
    private void GoToFriends()
    {
        if (!CheckCanAccessProtectedSection()) return;
        CloseGameBoardModalIfOpen();
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

    public void CommitAnimatedNavigation(ObservableObject? viewModel) =>
        DisplayedViewModel = viewModel;

    private void OnNavigationPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(INavigationService.CurrentViewModel))
            return;

        var next = _navigation.CurrentViewModel;
        if (_navigation.IsAnimatedTransition && DisplayedViewModel is not null)
            AnimatedNavigationRequested?.Invoke(next);
        else
            DisplayedViewModel = next;

        OnPropertyChanged(nameof(IsHeaderVisible));
        OnPropertyChanged(nameof(IsGuestRegisterVisible));
    }

    private void CloseGameBoardModalIfOpen()
    {
        if (_navigation.CurrentViewModel is GameBoardViewModel { ShowGameOver: true } gameBoard)
            gameBoard.CloseGameOverCommand.Execute(null);
    }

    private bool CheckCanAccessProtectedSection()
    {
        if (!_session.IsLoggedIn)
        {
            _dialogs.ShowMessage(LocalizationManager.Instance["Menu_Error_RequiresLogin"], icon: DialogIcon.Warning);
            return false;
        }

        if (_session.Current?.IsGuest == true)
        {
            _dialogs.ShowMessage(LocalizationManager.Instance["Menu_Error_RequiresFullAccount"], icon: DialogIcon.Warning);
            return false;
        }

        return true;
    }
}
