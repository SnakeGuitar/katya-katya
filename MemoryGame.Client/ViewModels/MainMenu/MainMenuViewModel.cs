using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Services.Core;
using MemoryGame.Client.Services.Interfaces;
using MemoryGame.Client.Services.Media;
using MemoryGame.Client.Services.Network;
using MemoryGame.Client.Services.UI;
using MemoryGame.Client.ViewModels.Lobby;
using MemoryGame.Client.ViewModels.Session;
using MemoryGame.Client.ViewModels.Settings;

namespace MemoryGame.Client.ViewModels.MainMenu;

/// <summary>
/// Main menu after login. Provides navigation to all game sections.
/// </summary>
public partial class MainMenuViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ISessionService _session;
    private readonly HubService _hub;

    private readonly string[] _moodImages =
    {
        "/Resources/Images/Backgrounds/katya-moods/main/katya-main-no-background.png",
        "/Resources/Images/Backgrounds/katya-moods/in-love/katya-in-love-no-background.png",
        "/Resources/Images/Backgrounds/katya-moods/shy/katya-shy-2-no-background.png"
    };

    [ObservableProperty]
    private string _currentMoodImage;

    public MainMenuViewModel(INavigationService navigation, ISessionService session, HubService hub)
    {
        _navigation = navigation;
        _session = session;
        _hub = hub;

        var index = new Random().Next(0, 3);
        _currentMoodImage = _moodImages[index];

    }

    public string Username => _session.Current?.Username ?? "Player";

    /// <summary>Formatted welcome string — avoids TwoWay binding issues with Run.Text.</summary>
    public string WelcomeMessage =>
        Localization.LocalizationManager.Instance.Format("Global_Message_Welcome", Username);

    [RelayCommand]
    private void GoToSettings() => _navigation.NavigateTo<SettingsViewModel>();

    [RelayCommand]
    private void GoToMultiplayer() => _navigation.NavigateTo<LobbyMenuViewModel>();

    [RelayCommand]
    private void GoToMore() => _navigation.NavigateTo<MoreMenuViewModel>();

    [RelayCommand]
    private void GoToStoryMode()
    {
        // Placeholder to move to Story Mode view
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _hub.DisconnectAsync();
        _session.EndSession();
        _navigation.NavigateToRootWithFade<TitleScreenViewModel>();
    }
}
