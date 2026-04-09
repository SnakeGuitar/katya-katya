namespace MemoryGame.Client.Services.Interfaces;

/// <summary>
/// Abstracts window-level operations (fullscreen toggle) so ViewModels
/// don't depend on System.Windows.Window directly.
/// </summary>
public interface IWindowService
{
    bool IsFullscreen { get; }
    void SetFullscreen(bool fullscreen);
    void ToggleFullscreen();
}
