namespace KatyaKatya.Services.Interfaces;

/// <summary>
/// Plays and manages background music tracks.
/// </summary>
public interface IMusicService : IDisposable
{
    event Action? TracksChanged;

    IReadOnlyList<string> Tracks { get; }
    int CurrentTrackIndex { get; set; }
    bool IsEnabled { get; set; }
    double Volume { get; set; }
}
