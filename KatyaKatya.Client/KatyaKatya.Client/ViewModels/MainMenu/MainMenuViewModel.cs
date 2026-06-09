using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    private static readonly string[] MoodPaths = new[]
    {
        "avares://KatyaKatya/Resources/Images/Backgrounds/katya-moods/main/katya-main-no-background.png",
        "avares://KatyaKatya/Resources/Images/Backgrounds/katya-moods/in-love/katya-in-love-no-background.png",
        "avares://KatyaKatya/Resources/Images/Backgrounds/katya-moods/shy/katya-shy-2-no-background.png"
    };

    [ObservableProperty]
    private string _welcomeMessage = string.Empty;

    [ObservableProperty]
    private string _currentMoodImage = string.Empty;

    public string LogoPath => KatyaKatya.Helpers.LogoResolver.Resolve(_settings.LanguageCode, _settings.ThemeName);

    public MainMenuViewModel(INavigationService navigation, ISessionService session, HubService hub, KatyaKatya.Services.Core.ClientSettings settings)
    {
        _navigation = navigation;
        _session = session;
        _hub = hub;
        _settings = settings;

        WelcomeMessage = $"Welcome, {_session.Current?.Username ?? "Player"}!";
        PickMoodImage();
    }

    private void PickMoodImage()
    {
        CurrentMoodImage = MoodPaths[Random.Shared.Next(MoodPaths.Length)];
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
