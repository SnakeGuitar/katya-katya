using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KatyaKatya.Localization;
using KatyaKatya.Services.Interfaces;
using KatyaKatya.Services.Network;
using KatyaKatya.ViewModels.Session;
using KatyaKatya.ViewModels.Settings;
using KatyaKatya.ViewModels.Lobby;
using KatyaKatya.ViewModels.SinglePlayer;

namespace KatyaKatya.ViewModels.MainMenu;

/// <summary>
/// Main menu after login. Provides navigation to all game sections.
/// </summary>
public partial class MainMenuViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ISessionService _session;
    private readonly HubService _hub;
    private readonly KatyaKatya.Services.Core.ClientSettings _settings;
    private readonly KatyaKatya.Helpers.IThemeAssetService _themeAssets;

    [ObservableProperty]
    private string _welcomeMessage = string.Empty;

    [ObservableProperty]
    private string _currentMoodImage = string.Empty;

    private string _assetsThemeName = string.Empty;

    public string LogoPath => KatyaKatya.Helpers.LogoResolver.Resolve(_settings.LanguageCode, _settings.ThemeName);
    public string BackgroundPath => _themeAssets.MainMenuBackgroundPath(_settings.ThemeName);

    public MainMenuViewModel(
        INavigationService navigation,
        ISessionService session,
        HubService hub,
        KatyaKatya.Services.Core.ClientSettings settings,
        KatyaKatya.Helpers.IThemeAssetService themeAssets)
    {
        _navigation = navigation;
        _session = session;
        _hub = hub;
        _settings = settings;
        _themeAssets = themeAssets;

        WelcomeMessage = LocalizationManager.Instance.Format(
            "Global_Message_Welcome",
            _session.Current?.Username ?? "Player");
        PickMoodImage();
    }

    private void PickMoodImage()
    {
        var moodPaths = _themeAssets.MainMenuMoodImages(_settings.ThemeName);
        CurrentMoodImage = moodPaths[Random.Shared.Next(moodPaths.Count)];
        _assetsThemeName = _settings.ThemeName;
    }

    /// <summary>
    /// Re-resolves theme-dependent assets if the theme changed since they were picked.
    /// Called by the view on attach and on ThemeChanged, so a history-cached instance
    /// returning from Settings doesn't keep showing the previous theme's art.
    /// </summary>
    public void RefreshThemeAssets()
    {
        if (_assetsThemeName == _settings.ThemeName) return;

        PickMoodImage();
        OnPropertyChanged(nameof(LogoPath));
        OnPropertyChanged(nameof(BackgroundPath));
    }

    [RelayCommand]
    private void GoToSettings() => _navigation.NavigateTo<SettingsViewModel>();

    [RelayCommand]
    private void GoToSingleplayer() => _navigation.NavigateTo<SinglePlayerMenuViewModel>();

    [RelayCommand]
    private void GoToMultiplayer() => _navigation.NavigateTo<LobbyMenuViewModel>();

    [RelayCommand]
    private void GoToMore() => _navigation.NavigateTo<MoreMenuViewModel>();

    [RelayCommand]
    private void GoToDating()
    {
        // TODO: Navigate to DatingHubViewModel
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _hub.DisconnectAsync();
        _session.EndSession();
        _navigation.NavigateToRootWithFade<TitleScreenViewModel>();
    }
}
