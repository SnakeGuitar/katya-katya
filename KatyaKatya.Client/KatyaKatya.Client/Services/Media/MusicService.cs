using System.IO;
using LibVLCSharp.Shared;
using KatyaKatya.Services.Core;
using KatyaKatya.Services.Interfaces;

namespace KatyaKatya.Services.Media;

/// <summary>
/// Background music player backed by LibVLC.
/// Gracefully no-ops if native VLC libraries are not available on the current platform.
/// </summary>
public class MusicService : IMusicService
{
    private readonly ClientSettings _settings;
    private LibVLC? _libVlc;
    private MediaPlayer? _player;
    private string[] _tracks = [];
    private string[] _trackNames = [];
    private int _currentTrack;
    private bool _initialized;

    public event Action? TracksChanged;

    public MusicService(ClientSettings settings)
    {
        _settings = settings;
        // Asynchronously initialize to prevent blocking the UI thread (resolving the 10-second settings freeze)
        Task.Run(() => TryInitialize());
    }

    private void TryInitialize()
    {
        try
        {
            LibVLCSharp.Shared.Core.Initialize();
            _libVlc = new LibVLC(enableDebugLogs: false);
            _player = new MediaPlayer(_libVlc);
            _player.EndReached += OnTrackEnded;

            var musicDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Music");
            if (!Directory.Exists(musicDir))
                musicDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Music");

            if (Directory.Exists(musicDir))
            {
                var all = Directory.GetFiles(musicDir, "*.mp3");

                // Prioritize すじをえがく if present
                var priority = all.FirstOrDefault(t =>
                    Path.GetFileName(t).Contains("すじをえがく", StringComparison.OrdinalIgnoreCase));

                _tracks = priority is null
                    ? all
                    : [priority, .. all.Where(t => t != priority)];

                _trackNames = _tracks
                    .Select(t => Path.GetFileNameWithoutExtension(t))
                    .ToArray();
            }

            TracksChanged?.Invoke();

            _player.Volume = (int)(_settings.MusicVolume * 100);
            _initialized = true;

            if (_settings.MusicEnabled && _tracks.Length > 0)
                PlayCurrent();
        }
        catch (Exception ex)
        {
            // VLC native libs not available on this platform — degrade silently
            System.Diagnostics.Debug.WriteLine($"[MusicService] VLC unavailable: {ex.Message}");
            _initialized = false;
        }
    }

    public IReadOnlyList<string> Tracks => _trackNames;

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
            if (!_initialized) return;
            if (value) PlayCurrent();
            else _player?.Stop();
        }
    }

    public double Volume
    {
        get => _settings.MusicVolume;
        set
        {
            _settings.MusicVolume = value;
            if (_initialized && _player is not null)
                _player.Volume = (int)(value * 100);
        }
    }

    private void PlayCurrent()
    {
        if (!_initialized || _player is null || _tracks.Length == 0) return;
        using var media = new LibVLCSharp.Shared.Media(_libVlc!, new Uri(_tracks[_currentTrack]));
        _player.Play(media);
    }

    private void OnTrackEnded(object? sender, EventArgs e)
    {
        // EndReached fires on a VLC thread — marshal back to ensure thread-safety
        _currentTrack = (_currentTrack + 1) % Math.Max(_tracks.Length, 1);
        // Small delay to let VLC clean up the previous media before opening the next
        Task.Delay(100).ContinueWith(_ =>
        {
            if (_settings.MusicEnabled) PlayCurrent();
        });
    }

    public void Dispose()
    {
        _player?.Stop();
        _player?.Dispose();
        _libVlc?.Dispose();
    }
}
