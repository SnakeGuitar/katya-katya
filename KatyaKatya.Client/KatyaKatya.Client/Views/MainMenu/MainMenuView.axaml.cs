using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using KatyaKatya.Helpers;
using KatyaKatya.ViewModels.MainMenu;

namespace KatyaKatya.Views.MainMenu;

public partial class MainMenuView : UserControl
{
    public MainMenuView()
    {
        InitializeComponent();

        // The theme is switched while Settings is on screen, so refresh both when the
        // event fires live and when this (history-cached) view is re-attached.
        AttachedToVisualTree += (_, _) =>
        {
            ThemeAssets.ThemeChanged += OnThemeChanged;
            OnThemeChanged();
            PetalCanvas.Start();
        };
        DetachedFromVisualTree += (_, _) =>
        {
            ThemeAssets.ThemeChanged -= OnThemeChanged;
            PetalCanvas.Stop();
        };

        ButtonPanelBorder.PointerMoved += OnPanelPointerMoved;
        ButtonPanelBorder.PointerEntered += (_, _) => GlowOverlay.Opacity = 1;
        ButtonPanelBorder.PointerExited += (_, _) => GlowOverlay.Opacity = 0;
    }

    private void OnThemeChanged() =>
        (DataContext as MainMenuViewModel)?.RefreshThemeAssets();

    private void OnPanelPointerMoved(object? sender, PointerEventArgs e)
    {
        if (GlowOverlay.Background is not RadialGradientBrush brush)
            return;

        var pos = e.GetPosition(ButtonPanelBorder);
        brush.Center = new RelativePoint(pos.X, pos.Y, RelativeUnit.Absolute);
        brush.GradientOrigin = new RelativePoint(pos.X, pos.Y, RelativeUnit.Absolute);
    }
}
