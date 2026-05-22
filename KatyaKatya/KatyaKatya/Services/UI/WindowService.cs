using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using KatyaKatya.Services.Interfaces;

namespace KatyaKatya.Services.UI;

/// <summary>
/// Avalonia implementation of window-level operations.
/// </summary>
public class WindowService : IWindowService
{
    private bool _isFullscreen;
    private WindowState _previousState = WindowState.Normal;

    public bool IsFullscreen => _isFullscreen;

    public void SetFullscreen(bool fullscreen)
    {
        var window = GetMainWindow();
        if (window is null) return;

        if (fullscreen)
        {
            _previousState = window.WindowState;
            window.WindowState = WindowState.FullScreen;
            _isFullscreen = true;
        }
        else
        {
            window.WindowState = _previousState;
            _isFullscreen = false;
        }
    }

    public void ToggleFullscreen() => SetFullscreen(!_isFullscreen);

    private static Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;
        return null;
    }
}
