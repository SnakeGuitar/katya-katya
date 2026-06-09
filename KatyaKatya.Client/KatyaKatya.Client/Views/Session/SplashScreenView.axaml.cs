using Avalonia.Controls;
using KatyaKatya.ViewModels.Session;

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
                vm.FadeOutRequested += () =>
                {
                    Opacity = 0;
                    vm.NavigateToTitleScreen();
                };
                await vm.StartAsync();
            }
        };
    }
}
