using System.Windows;

namespace MemoryGame.Client.Services;

public class WindowService : IWindowService
{
    private bool _isFullscreen;

    public bool IsFullscreen => _isFullscreen;

    public void SetFullscreen(bool fullscreen)
    {
        var window = Application.Current.MainWindow;
        if (window is null) return;

        _isFullscreen = fullscreen;
        
        if (fullscreen)
        {
            window.WindowState = WindowState.Maximized;
        }
        else
        {
            window.WindowState = WindowState.Normal;
        }
    }

    public void ToggleFullscreen() => SetFullscreen(!IsFullscreen);
}
