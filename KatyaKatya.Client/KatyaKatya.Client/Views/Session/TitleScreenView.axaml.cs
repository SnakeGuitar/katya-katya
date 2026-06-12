using Avalonia.Controls;

namespace KatyaKatya.Views.Session;

public partial class TitleScreenView : UserControl
{
    public TitleScreenView()
    {
        InitializeComponent();
        AttachedToVisualTree += (_, _) => PetalCanvas.Start();
        DetachedFromVisualTree += (_, _) => PetalCanvas.Stop();
    }
}
