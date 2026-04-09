using CommunityToolkit.Mvvm.ComponentModel;

namespace MemoryGame.Client.ViewModels.Lobby;

/// <summary>
/// ViewModel for the host lobby screen. Placeholder — will be implemented next.
/// </summary>
public partial class HostLobbyViewModel : ObservableObject
{
    [ObservableProperty]
    private string _gameCode = string.Empty;
}
