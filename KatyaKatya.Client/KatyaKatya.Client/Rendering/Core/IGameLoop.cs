namespace KatyaKatya.Rendering.Core;

/// <summary>
/// The single frame ticker for the whole client. Owns one timer, computes delta and
/// smoothed frame time, drives every registered <see cref="IFrameUpdatable"/>, and parks
/// itself when nothing is animating. Replaces the per-control <c>DispatcherTimer</c>s.
/// </summary>
public interface IGameLoop
{
    /// <summary>Register a system to receive ticks. Idempotent; also wakes the loop.</summary>
    void Register(IFrameUpdatable system);

    /// <summary>Stop ticking a system. Idempotent.</summary>
    void Unregister(IFrameUpdatable system);

    /// <summary>
    /// Restart the timer if it was parked. Systems call this when they begin animating
    /// (e.g. a particle burst) so the loop notices them again after a quiet period.
    /// </summary>
    void Wake();

    /// <summary>Snapshot of the most recently produced frame.</summary>
    FrameTime LastFrame { get; }

    /// <summary>Number of systems currently reporting <see cref="IFrameUpdatable.IsActive"/>.</summary>
    int ActiveSystemCount { get; }

    /// <summary>All registered systems, in registration order (for diagnostics).</summary>
    IReadOnlyList<IFrameUpdatable> Systems { get; }

    /// <summary>
    /// Free-form label for the active scene/screen, included in slow-frame logs.
    /// </summary>
    string? CurrentContext { get; set; }

    /// <summary>Raised after each frame the loop produces.</summary>
    event Action<FrameTime>? FrameCompleted;

    /// <summary>Raised when a frame exceeds the slow-frame threshold (24 ms).</summary>
    event Action<FrameTime>? SlowFrame;
}
