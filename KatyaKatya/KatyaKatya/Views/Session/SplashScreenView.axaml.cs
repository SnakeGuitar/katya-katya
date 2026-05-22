using Avalonia.Controls;
using KatyaKatya.ViewModels.Session;
using System.Threading.Tasks;

namespace KatyaKatya.Views.Session;

public partial class SplashScreenView : UserControl
{
    public SplashScreenView()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            if (DataContext is SplashScreenViewModel vm)
            {
                vm.FadeOutRequested += async () =>
                {
                    // Trigger Fade Out of the whole view
                    RootGrid.Opacity = 0;
                    // Wait for the duration of the transition (1.2 seconds = 1200ms)
                    await Task.Delay(1200);
                    vm.NavigateToTitleScreen();
                };

                await vm.StartAsync();
            }
        };
    }
}
