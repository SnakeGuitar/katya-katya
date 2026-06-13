using System.IO;
using LibVLCSharp.Shared;
using KatyaKatya.Services.Interfaces;

namespace KatyaKatya.Services.Media;

/// <summary>
/// Plays short UI sound effects via LibVLC. Each effect has its own pre-warmed
/// <see cref="MediaPlayer"/> so rapid hovers restart cleanly without cutting off
/// the music. Degrades silently if the native VLC libraries are unavailable.
/// </summary>
public class SoundService : ISoundService
{
    private LibVLC? _libVlc;
    private MediaPlayer? _hoverPlayer;
    private MediaPlayer? _clickPlayer;
    private string? _hoverPath;
    private string? _clickPath;
    private bool _initialized;

    public bool IsEnabled { get; set; } = true;

    public SoundService()
    {
        // Initialize off the UI thread so we never block startup.
        Task.Run(TryInitialize);
    }

    private void TryInitialize()
    {
        try
        {
            LibVLCSharp.Shared.Core.Initialize();
            _libVlc = new LibVLC(enableDebugLogs: false);

            var soundsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Sounds");
            if (!Directory.Exists(soundsDir))
                soundsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds");

            _hoverPath = Path.Combine(soundsDir, "hover.wav");
            _clickPath = Path.Combine(soundsDir, "click.wav");

            // Hover should sit clearly below the music; the click is a touch louder.
            _hoverPlayer = new MediaPlayer(_libVlc) { Volume = 45 };
            _clickPlayer = new MediaPlayer(_libVlc) { Volume = 70 };

            _initialized = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SoundService] VLC unavailable: {ex.Message}");
            _initialized = false;
        }
    }

    public void PlayHover() => Play(_hoverPlayer, _hoverPath);

    public void PlayClick() => Play(_clickPlayer, _clickPath);

    private void Play(MediaPlayer? player, string? path)
    {
        if (!IsEnabled || !_initialized || player is null || _libVlc is null
            || path is null || !File.Exists(path))
            return;

        try
        {
            player.Stop();
            using var media = new LibVLCSharp.Shared.Media(_libVlc, new Uri(path));
            player.Play(media);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SoundService] play failed: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _hoverPlayer?.Dispose();
        _clickPlayer?.Dispose();
        _libVlc?.Dispose();
    }
}
