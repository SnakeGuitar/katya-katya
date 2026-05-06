using CommunityToolkit.Mvvm.ComponentModel;

namespace MemoryGame.Client.Models.Lobby;

/// <summary>
/// Tracks a player's name, score, and turn state for the multiplayer game board UI.
/// </summary>
public partial class PlayerScoreViewModel : ObservableObject
{
    public string Username { get; }
    public bool IsCurrentUser { get; }

    [ObservableProperty]
    private int _score;

    [ObservableProperty]
    private bool _isActive;

    [ObservableProperty]
    private string _timeDisplay = "--";

    public PlayerScoreViewModel(string username, bool isCurrentUser)
    {
        Username = username;
        IsCurrentUser = isCurrentUser;
    }
}
