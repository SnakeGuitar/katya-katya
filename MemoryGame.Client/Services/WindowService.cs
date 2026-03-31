using System.Windows;

namespace MemoryGame.Client.Services;

public class WindowService : IWindowService
{
    public bool IsFullscreen =>
        Application.Current.MainWindow?.WindowStyle == WindowStyle.None;

    public void SetFullscreen(bool fullscreen)
    {
        var window = Application.Current.MainWindow;
        if (window is null) return;

        if (fullscreen)
        {
            window.WindowStyle = WindowStyle.None;
            window.WindowState = WindowState.Maximized;
        }
        else
        {
            window.WindowStyle = WindowStyle.SingleBorderWindow;
            window.WindowState = WindowState.Normal;
        }
    }

    public void ToggleFullscreen() => SetFullscreen(!IsFullscreen);
}
