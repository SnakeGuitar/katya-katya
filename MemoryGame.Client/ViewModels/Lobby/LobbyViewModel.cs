using CommunityToolkit.Mvvm.ComponentModel;

namespace MemoryGame.Client.ViewModels.Lobby;

/// <summary>
/// ViewModel for the player (non-host) lobby screen. Placeholder — will be implemented next.
/// </summary>
public partial class LobbyViewModel : ObservableObject
{
    [ObservableProperty]
    private string _gameCode = string.Empty;
}
