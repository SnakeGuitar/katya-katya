using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MemoryGame.Client.Views.Controls.Effects;

/// <summary>
/// Base class for canvas-based particle effects.
/// </summary>
/// <remarks>
/// Manages the spawn timer and particle cap. Subclasses only need to implement
/// <see cref="SpawnParticle"/> and call <see cref="ScheduleRemoval"/> when done.
/// <code>
/// private readonly PetalParticleSystem _petals = new() { MaxParticles = 22 };
///
/// Loaded   += (_, _) => _petals.Attach(PetalCanvas);
/// Unloaded += (_, _) => _petals.Detach();
/// </code>
/// </remarks>
public abstract class ParticleSystem
{
    /// <summary>Shared RNG available to all subclasses.</summary>
    protected static readonly Random Rng = new();

    private DispatcherTimer? _timer;
    private Canvas?          _canvas;

    // ── configuration ────────────────────────────────────────────────────────

    /// <summary>
    /// Maximum number of live particles allowed on the canvas at once.
    /// Spawn attempts are skipped while this limit is reached.
    /// </summary>
    /// <value>Default: <c>20</c></value>
    public int MaxParticles { get; set; } = 20;

    /// <summary>
    /// Milliseconds between consecutive spawn attempts.
    /// Must be set before calling <see cref="Attach"/>.
    /// </summary>
    /// <value>Default: <c>650</c> ms</value>
    public int SpawnIntervalMs { get; set; } = 650;

    // ── lifecycle ────────────────────────────────────────────────────────────

    /// <summary>
    /// Attaches the effect to <paramref name="canvas"/> and starts the spawn timer.
    /// </summary>
    /// <param name="canvas">
    /// Host canvas. Should have <c>IsHitTestVisible="False"</c> to avoid
    /// intercepting mouse events.
    /// </param>
    public void Attach(Canvas canvas)
    {
        _canvas = canvas;
        _timer  = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(SpawnIntervalMs) };
        _timer.Tick += OnTick;
        _timer.Start();
    }

    /// <summary>
    /// Stops the spawn timer, clears all live particles, and releases the canvas.
    /// Safe to call multiple times.
    /// </summary>
    public void Detach()
    {
        _timer?.Stop();
        _timer = null;
        _canvas?.Children.Clear();
        _canvas = null;
    }

    // ── internal ─────────────────────────────────────────────────────────────

    private void OnTick(object? sender, EventArgs e)
    {
        if (_canvas is null) return;
        double w = _canvas.ActualWidth, h = _canvas.ActualHeight;
        if (w <= 0 || h <= 0) return;
        if (_canvas.Children.Count >= MaxParticles) return;

        SpawnParticle(_canvas, w, h);
    }

    // ── for subclasses ───────────────────────────────────────────────────────

    /// <summary>
    /// Creates one particle, adds it to <paramref name="canvas"/>, and starts its animations.
    /// End every implementation with a call to <see cref="ScheduleRemoval"/>.
    /// </summary>
    /// <param name="canvas">Target canvas.</param>
    /// <param name="canvasW">Canvas width in device-independent pixels.</param>
    /// <param name="canvasH">Canvas height in device-independent pixels.</param>
    protected abstract void SpawnParticle(Canvas canvas, double canvasW, double canvasH);

    /// <summary>
    /// Removes <paramref name="element"/> from <paramref name="canvas"/> after <paramref name="delay"/>.
    /// </summary>
    /// <param name="delay">
    /// Should be at least as long as the longest animation on <paramref name="element"/>.
    /// </param>
    protected static void ScheduleRemoval(Canvas canvas, UIElement element, TimeSpan delay)
    {
        var timer = new DispatcherTimer { Interval = delay };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            canvas.Children.Remove(element);
        };
        timer.Start();
    }
}
