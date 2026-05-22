using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace KatyaKatya.Views.MainMenu;

public partial class MainMenuView : UserControl
{
    public MainMenuView()
    {
        InitializeComponent();
    }

    private void NavCardBorder_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (sender is not Border border || GlowOverlay?.Background is not RadialGradientBrush brush) return;
        var pos = e.GetPosition(border);
        double relX = pos.X / border.Bounds.Width;
        double relY = pos.Y / border.Bounds.Height;
        brush.Center = new RelativePoint(relX, relY, RelativeUnit.Relative);
        brush.GradientOrigin = new RelativePoint(relX, relY, RelativeUnit.Relative);
    }

    private void NavCardBorder_PointerEntered(object? sender, PointerEventArgs e)
    {
        if (GlowOverlay is not null)
            GlowOverlay.Opacity = 1.0;
    }

    private void NavCardBorder_PointerExited(object? sender, PointerEventArgs e)
    {
        if (GlowOverlay is not null)
            GlowOverlay.Opacity = 0.0;
    }
}
