namespace KatyaKatya.Services.Interfaces;

/// <summary>
/// Abstracts window-level operations so ViewModels don't depend on platform Window directly.
/// </summary>
public interface IWindowService
{
    bool IsFullscreen { get; }
    void SetFullscreen(bool fullscreen);
    void ToggleFullscreen();
}
