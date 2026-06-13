namespace KatyaKatya.Rendering.Core;

/// <summary>
/// Immutable snapshot of timing for a single frame, produced by <see cref="IGameLoop"/>
/// and passed to every <see cref="IFrameUpdatable"/>. Systems must integrate against
/// <see cref="DeltaSeconds"/> rather than measuring wall-clock time themselves.
/// </summary>
public readonly record struct FrameTime
{
    /// <summary>Seconds elapsed since the previous frame, clamped to a sane range.</summary>
    public double DeltaSeconds { get; init; }

    /// <summary>Exponentially smoothed delta, useful for jitter-free FPS readouts.</summary>
    public double SmoothedDeltaSeconds { get; init; }

    /// <summary>Seconds elapsed since the loop started running.</summary>
    public double TotalSeconds { get; init; }

    /// <summary>Monotonic frame counter since the loop started.</summary>
    public long FrameIndex { get; init; }

    /// <summary>Instantaneous frames per second derived from <see cref="DeltaSeconds"/>.</summary>
    public double Fps => DeltaSeconds > 0 ? 1.0 / DeltaSeconds : 0;

    /// <summary>Smoothed frames per second derived from <see cref="SmoothedDeltaSeconds"/>.</summary>
    public double SmoothedFps => SmoothedDeltaSeconds > 0 ? 1.0 / SmoothedDeltaSeconds : 0;

    /// <summary>Smoothed frame duration in milliseconds.</summary>
    public double SmoothedFrameMs => SmoothedDeltaSeconds * 1000.0;
}
