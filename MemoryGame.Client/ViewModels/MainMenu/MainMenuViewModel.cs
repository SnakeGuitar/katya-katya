using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MemoryGame.Client.Messages;
using MemoryGame.Client.Services.Interfaces;
using MemoryGame.Client.Services.Network;
using MemoryGame.Client.Services.UI;
using MemoryGame.Client.ViewModels.Lobby;
using MemoryGame.Client.ViewModels.Session;
using MemoryGame.Client.ViewModels.Settings;

namespace MemoryGame.Client.ViewModels.MainMenu;

/// <summary>
/// Main menu after login. Provides navigation to all game sections.
/// </summary>
public partial class MainMenuViewModel : ObservableObject, IRecipient<ThemeChangedMessage>
{
    private readonly INavigationService _navigation;
    private readonly ISessionService _session;
    private readonly HubService _hub;

    [ObservableProperty]
    private string _currentMoodImage = "";

    [ObservableProperty]
    private double _imageScale = 1.0;

    public MainMenuViewModel(INavigationService navigation, ISessionService session, HubService hub)
    {
        _navigation = navigation;
        _session = session;
        _hub = hub;

        PickMoodImage();
        WeakReferenceMessenger.Default.Register(this);
    }

    public void Receive(ThemeChangedMessage message) => PickMoodImage();

    private void PickMoodImage()
    {
        var paths = ThemeService.CurrentAssets.MainMenuMoodImages;
        CurrentMoodImage = paths[Random.Shared.Next(paths.Count)];
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
