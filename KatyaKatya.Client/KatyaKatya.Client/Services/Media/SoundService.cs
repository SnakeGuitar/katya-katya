using System.IO;
using LibVLCSharp.Shared;
using KatyaKatya.Engine.Settings;
using KatyaKatya.Services.Interfaces;
using VlcMedia = LibVLCSharp.Shared.Media;

namespace KatyaKatya.Services.Media;

/// <summary>
/// Plays short UI sound effects via LibVLC. Each effect has its own pre-warmed
/// <see cref="MediaPlayer"/> so rapid hovers restart cleanly without cutting off
/// the music. Degrades silently if the native VLC libraries are unavailable.
/// </summary>
public class SoundService : ISoundService
{
    private static readonly TimeSpan HoverThrottle = TimeSpan.FromMilliseconds(45);

    private readonly IGraphicsSettingsService _graphicsSettings;
    private LibVLC? _libVlc;
    private MediaPlayer? _hoverPlayer;
    private MediaPlayer? _clickPlayer;
    private VlcMedia? _hoverMedia;
    private VlcMedia? _clickMedia;
    private bool _initialized;
    private DateTime _lastHoverAttempt = DateTime.MinValue;

    public bool IsEnabled { get; set; } = true;
    public int FailureCount { get; private set; }
    public DateTime? LastHoverAt { get; private set; }
    public DateTime? LastClickAt { get; private set; }
    public string DebugMetrics =>
        $"sfx enabled:{IsEnabled && _graphicsSettings.EnableUiSfx} failures:{FailureCount}";

    public SoundService(IGraphicsSettingsService graphicsSettings)
    {
        _graphicsSettings = graphicsSettings;
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

            var hoverPath = Path.Combine(soundsDir, "hover.wav");
            var clickPath = Path.Combine(soundsDir, "click.wav");

            // Hover should sit clearly below the music; the click is a touch louder.
            _hoverPlayer = new MediaPlayer(_libVlc) { Volume = 45 };
            _clickPlayer = new MediaPlayer(_libVlc) { Volume = 70 };

            // Pre-create one Media per effect and keep it alive for the lifetime of the
            // service. Creating a Media per play and disposing it while LibVLC was still
            // playing it asynchronously caused a native use-after-free (0xC0000005) on the
            // next Stop(); reusing a long-lived Media also avoids per-event allocation.
            if (File.Exists(hoverPath))
                _hoverMedia = new VlcMedia(_libVlc, new Uri(hoverPath));
            if (File.Exists(clickPath))
                _clickMedia = new VlcMedia(_libVlc, new Uri(clickPath));

            _initialized = true;
        }
        catch (Exception ex)
        {
            FailureCount++;
            System.Diagnostics.Debug.WriteLine($"[SoundService] VLC unavailable: {ex.Message}");
            _initialized = false;
        }
    }

    public void PlayHover()
    {
        var now = DateTime.UtcNow;
        if (now - _lastHoverAttempt < HoverThrottle)
            return;

        _lastHoverAttempt = now;
        LastHoverAt = now;
        Play(_hoverPlayer, _hoverMedia);
    }

    public void PlayClick()
    {
        LastClickAt = DateTime.UtcNow;
        Play(_clickPlayer, _clickMedia);
    }

    private void Play(MediaPlayer? player, VlcMedia? media)
    {
        if (!IsEnabled || !_graphicsSettings.EnableUiSfx || !_initialized || player is null || media is null)
            return;

        // LibVLC player controls must not run on the UI thread (Stop() blocks and can
        // deadlock). Hand the work to a background thread, serialized per player so two
        // rapid events can't drive concurrent native Stop()/Play() on the same player.
        Task.Run(() =>
        {
            try
            {
                lock (player)
                {
                    player.Stop();
                    player.Play(media);
                }
            }
            catch (Exception ex)
            {
                FailureCount++;
                System.Diagnostics.Debug.WriteLine($"[SoundService] play failed: {ex.Message}");
            }
        });
    }

    public void Dispose()
    {
        _hoverPlayer?.Dispose();
        _clickPlayer?.Dispose();
        _hoverMedia?.Dispose();
        _clickMedia?.Dispose();
        _libVlc?.Dispose();
    }
}
