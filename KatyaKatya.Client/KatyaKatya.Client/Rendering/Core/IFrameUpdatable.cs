namespace KatyaKatya.Rendering.Core;

/// <summary>
/// A simulation/animation system driven by the single shared <see cref="IGameLoop"/>.
/// Implementers must not own their own timer; they advance state inside <see cref="Tick"/>.
/// </summary>
public interface IFrameUpdatable
{
    /// <summary>
    /// True while this system has live animation that needs ticking. When every
    /// registered system reports false, the loop parks its timer until something
    /// calls <see cref="IGameLoop.Wake"/> again.
    /// </summary>
    bool IsAnimating { get; }

    /// <summary>Advance one frame using the shared <paramref name="time"/>.</summary>
    void Tick(in FrameTime time);
}

/// <summary>
/// Optional companion to <see cref="IFrameUpdatable"/>: lets a system surface a short
/// human-readable status line (e.g. live particle counts) for the debug overlay.
/// </summary>
public interface IFrameDebugMetrics
{
    /// <summary>A compact one-line metric string, or null when there is nothing to show.</summary>
    string? DebugMetrics { get; }
}
