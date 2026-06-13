using System.Windows;

namespace MemoryGame.Client.Services.UI;
using MemoryGame.Client.Services.Interfaces;

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
            window.WindowState = WindowState.Normal;
            window.ResizeMode  = ResizeMode.NoResize;
            window.Topmost     = true;
            
            window.Left   = 0;
            window.Top    = 0;
            window.Width  = SystemParameters.PrimaryScreenWidth;
            window.Height = SystemParameters.PrimaryScreenHeight;
        }
        else
        {
            window.Topmost      = false;
            window.ResizeMode   = ResizeMode.CanResize;
            window.WindowState  = WindowState.Normal;
  
            window.Width  = 1280;
            window.Height = 720;
            
            var screenWidth  = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            window.Left = (screenWidth - window.Width) / 2;
            window.Top  = (screenHeight - window.Height) / 2;
        }
    }

    public void ToggleFullscreen() => SetFullscreen(!IsFullscreen);
}
