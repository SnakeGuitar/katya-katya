using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace KatyaKatya.Models.Lobby;

/// <summary>
/// Tracks a player's name, score, and turn state for the multiplayer/singleplayer game board UI.
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

    /// <summary>
    /// Toggled off/on around each score change so the view's ScorePulse
    /// class (and its attached animation) re-runs on every update.
    /// </summary>
    [ObservableProperty]
    private bool _scorePulseActive;

    public PlayerScoreViewModel(string username, bool isCurrentUser)
    {
        Username = username;
        IsCurrentUser = isCurrentUser;
    }

    partial void OnScoreChanged(int value)
    {
        ScorePulseActive = false;
        Dispatcher.UIThread.Post(() => ScorePulseActive = true);
    }
}
