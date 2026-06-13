namespace KatyaKatya.Services.Interfaces;

/// <summary>
/// Plays short UI sound effects (button hover / click).
/// </summary>
public interface ISoundService : IDisposable
{
    bool IsEnabled { get; set; }

    /// <summary>Plays the soft hover tick.</summary>
    void PlayHover();

    /// <summary>Plays the click / press pop.</summary>
    void PlayClick();
}
