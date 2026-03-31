using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Services;
using MemoryGame.Client.ViewModels.Session;

namespace MemoryGame.Client.ViewModels.MainMenu;

/// <summary>
/// Main menu after login. Provides navigation to all game sections.
/// </summary>
public partial class MainMenuViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ISessionService _session;
    private readonly HubService _hub;

    public MainMenuViewModel(INavigationService navigation, ISessionService session, HubService hub)
    {
        _navigation = navigation;
        _session = session;
        _hub = hub;
    }

    public string Username => _session.Current?.Username ?? "Player";

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _hub.DisconnectAsync();
        _session.EndSession();
        _navigation.NavigateTo<TitleScreenViewModel>();
    }
}
