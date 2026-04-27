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
public partial class MainMenuViewModel : ObservableObject, CommunityToolkit.Mvvm.Messaging.IRecipient<Messages.ThemeChangedMessage>
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

    private readonly string[] _sketchMoodImages =
    {
        "/Resources/Images/Backgrounds/katya-moods/main/sketch-katya-main-no-background.png",
        "/Resources/Images/Backgrounds/katya-moods/in-love/sketch-katya-in-love-no-background.png",
        "/Resources/Images/Backgrounds/katya-moods/shy/sketch-katya-shy-no-background.png",
        "/Resources/Images/Backgrounds/katya-moods/standing/sketch-katya-standing-no-background.png"
    };

    [ObservableProperty]
    private string _currentMoodImage;

    [ObservableProperty]
    private double _imageScale = 1.0;

    public MainMenuViewModel(INavigationService navigation, ISessionService session, HubService hub, ClientSettings settings)
    {
        _navigation = navigation;
        _session = session;
        _hub = hub;

        if (settings.ThemeName == "Sketch")
        {
            _currentMoodImage = _sketchMoodImages[new Random().Next(_sketchMoodImages.Length)];
            _imageScale = 1.0;
        }
        else
        {
            _currentMoodImage = _moodImages[new Random().Next(_moodImages.Length)];
            _imageScale = 1.0;
        }

        CommunityToolkit.Mvvm.Messaging.IMessengerExtensions.Register(CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default, this);
    }

    public void Receive(Messages.ThemeChangedMessage message)
    {
        CurrentMoodImage = message.ThemeName == "Sketch"
            ? _sketchMoodImages[Random.Shared.Next(_sketchMoodImages.Length)]
            : _moodImages[Random.Shared.Next(_moodImages.Length)];
        ImageScale = 1.0;
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
