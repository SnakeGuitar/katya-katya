using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using MemoryGame.Client.ViewModels.Session;

namespace MemoryGame.Client.Views.Session;

public partial class SplashScreenView : UserControl
{
    public SplashScreenView()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SplashScreenViewModel vm) return;

        vm.FadeOutRequested += () =>
        {
            Dispatcher.Invoke(() =>
            {
                var fadeOut = (Storyboard)Resources["FadeOut"];
                fadeOut.Completed += (_, _) => vm.NavigateToTitleScreen();
                fadeOut.Begin(this);
            });
        };

        var fadeIn = (Storyboard)Resources["FadeIn"];
        fadeIn.Begin(this);
        _ = vm.StartAsync();
    }
}
