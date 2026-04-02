using System.Windows.Controls;
using System.Windows.Input;
using MemoryGame.Client.ViewModels.Gallery;

namespace MemoryGame.Client.Views.Gallery;

public partial class GalleryView : UserControl
{
    public GalleryView()
    {
        InitializeComponent();
    }

    private void OnOverlayBackgroundMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is GalleryViewModel vm)
            vm.CloseCardCommand.Execute(null);
    }

    private void OnExpandedImageMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is GalleryViewModel vm)
            vm.SelectedCard?.CycleCommand.Execute(null);

        e.Handled = true;
    }
}
