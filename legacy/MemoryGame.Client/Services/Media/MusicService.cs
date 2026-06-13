using System.IO;
using System.Windows.Media;

namespace MemoryGame.Client.Services.Media;
using MemoryGame.Client.Services.Core;

/// <summary>
/// Plays background music by cycling through MP3 tracks in Resources/Music.
/// Volume and enabled state persist via ClientSettings.
/// Supports manual track selection.
/// </summary>
public class MusicService : IDisposable
{
    private readonly ClientSettings _settings;
    private readonly MediaPlayer _player = new();
    private string[] _tracks = [];
    private string[] _trackNames = [];
    private int _currentTrack;

    public MusicService(ClientSettings settings)
    {
        _settings = settings;

        var musicDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Music");
        if (Directory.Exists(musicDir))
        {
            var all = Directory.GetFiles(musicDir, "*.mp3");

            var sujioegaku = all.FirstOrDefault(t =>
                Path.GetFileName(t).Contains("すじをえがく", StringComparison.OrdinalIgnoreCase));

            _tracks = sujioegaku is null
                ? all
                : [sujioegaku, .. all.Where(t => t != sujioegaku)];

            _trackNames = _tracks
                .Select(t => Path.GetFileNameWithoutExtension(t))
                .ToArray();
        }

        _player.MediaEnded += OnTrackEnded;
        _player.Volume = settings.MusicVolume;

        if (settings.MusicEnabled && _tracks.Length > 0)
            PlayCurrent();
    }

    /// <summary>Get read-only list of available track names.</summary>
    public IReadOnlyList<string> Tracks => _trackNames;

    /// <summary>Get or set the current track index.</summary>
    public int CurrentTrackIndex
    {
        get => _currentTrack;
        set
        {
            if (value >= 0 && value < _tracks.Length)
            {
                _currentTrack = value;
                if (IsEnabled) PlayCurrent();
            }
        }
    }

    public bool IsEnabled
    {
        get => _settings.MusicEnabled;
        set
        {
            _settings.MusicEnabled = value;
            if (value) PlayCurrent();
            else _player.Stop();
        }
    }

    public double Volume
    {
        get => _settings.MusicVolume;
        set
        {
            _settings.MusicVolume = value;
            _player.Volume = value;
        }
    }

    private void PlayCurrent()
    {
        if (_tracks.Length == 0) return;
        _player.Open(new Uri(_tracks[_currentTrack], UriKind.Absolute));
        _player.Volume = _settings.MusicVolume;
        _player.Play();
    }

    private void OnTrackEnded(object? sender, EventArgs e)
    {
        _currentTrack = (_currentTrack + 1) % Math.Max(_tracks.Length, 1);
        if (IsEnabled) PlayCurrent();
    }

    public void Dispose()
    {
        _player.Stop();
        _player.Close();
    }
}
