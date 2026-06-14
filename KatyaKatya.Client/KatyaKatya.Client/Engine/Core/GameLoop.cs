using System.Diagnostics;
using Avalonia.Threading;

namespace KatyaKatya.Engine.Core;

/// <summary>
/// Single UI-thread frame ticker built on one <see cref="DispatcherTimer"/> plus a
/// <see cref="Stopwatch"/> for accurate deltas. It ticks every registered
/// <see cref="IFrameUpdatable"/>, smooths frame time for readouts, parks itself when no
/// system is animating, and re-arms on <see cref="Wake"/>.
/// </summary>
public sealed class GameLoop : IGameLoop
{
    // ~60 Hz. Real delta comes from the stopwatch, so a slightly imprecise timer is fine.
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(16);

    // Delta is clamped so a stall (debugger break, GC pause, tab switch) can't teleport
    // simulations across a huge time step.
    private const double MaxDeltaSeconds = 0.05;

    // Frames slower than this are surfaced for profiling (spec: log frames > 24 ms).
    private const double SlowFrameSeconds = 0.024;

    // EMA weight for the smoothed frame time shown in the overlay.
    private const double SmoothingAlpha = 0.1;

    private readonly DispatcherTimer _timer;
    private readonly Stopwatch _clock = new();
    private readonly List<IFrameUpdatable> _systems = [];

    private double _smoothedDelta;
    private long _frameIndex;
    private double _lastTotalSeconds;
    // Keep ticking briefly after the last animating system goes idle so trailing
    // invalidations flush, then park.
    private int _idleGraceFrames;

    public GameLoop()
    {
        _timer = new DispatcherTimer(DispatcherPriority.Render) { Interval = Interval };
        _timer.Tick += OnTick;
    }

    public FrameTime LastFrame { get; private set; }

    public int ActiveSystemCount
    {
        get
        {
            var count = 0;
            foreach (var system in _systems)
                if (system.IsActive)
                    count++;
            return count;
        }
    }

    public IReadOnlyList<IFrameUpdatable> Systems => _systems;

    public string? CurrentContext { get; set; }

    public event Action<FrameTime>? FrameCompleted;
    public event Action<FrameTime>? SlowFrame;

    public void Register(IFrameUpdatable system)
    {
        if (!_systems.Contains(system))
            _systems.Add(system);
        Wake();
    }

    public void Unregister(IFrameUpdatable system)
    {
        _systems.Remove(system);
        if (_systems.Count == 0)
            Park();
    }

    public void Wake()
    {
        _idleGraceFrames = 6;
        if (_timer.IsEnabled)
            return;

        _clock.Restart();
        _lastTotalSeconds = 0;
        LastFrame = LastFrame with { DeltaSeconds = 0 };
        _timer.Start();
    }

    private void Park()
    {
        _timer.Stop();
        _clock.Stop();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        var total = _clock.Elapsed.TotalSeconds;
        var delta = Math.Clamp(total - _lastTotalSeconds, 0, MaxDeltaSeconds);
        _lastTotalSeconds = total;

        _smoothedDelta = _smoothedDelta <= 0
            ? delta
            : _smoothedDelta + (delta - _smoothedDelta) * SmoothingAlpha;

        var frame = new FrameTime
        {
            DeltaSeconds = delta,
            SmoothedDeltaSeconds = _smoothedDelta,
            TotalSeconds = total,
            FrameIndex = _frameIndex++,
        };
        LastFrame = frame;

        var anyAnimating = false;
        // Snapshot guard: a system may unregister itself during Tick.
        for (var i = _systems.Count - 1; i >= 0; i--)
        {
            if (i >= _systems.Count)
                continue;
            var system = _systems[i];
            if (system.IsActive)
            {
                anyAnimating = true;
                system.Tick(in frame);
            }
        }

        FrameCompleted?.Invoke(frame);

        if (delta >= SlowFrameSeconds)
        {
            SlowFrame?.Invoke(frame);
            Debug.WriteLine(
                $"[GameLoop] slow frame {delta * 1000:F1} ms (scene: {CurrentContext ?? "?"}, systems: {ActiveSystemCount})");
        }

        if (anyAnimating)
            _idleGraceFrames = 6;
        else if (--_idleGraceFrames <= 0)
            Park();
    }
}
