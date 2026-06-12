using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Styling;
using KatyaKatya.ViewModels.Session;

namespace KatyaKatya.Views.Session;

public partial class SplashScreenView : UserControl
{
    private bool _started;

    public SplashScreenView()
    {
        InitializeComponent();

        Loaded += OnLoaded;
        DetachedFromVisualTree += (_, _) =>
        {
            if (DataContext is SplashScreenViewModel vm)
                vm.FadeOutRequested -= OnFadeOutRequested;
        };
    }

    private async void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_started || DataContext is not SplashScreenViewModel vm)
            return;

        _started = true;
        vm.FadeOutRequested += OnFadeOutRequested;
        await AnimateOpacityAsync(LogoContainer, 0, 1, 1100);
        await vm.StartAsync();
    }

    private void OnFadeOutRequested()
    {
        if (DataContext is SplashScreenViewModel vm)
            vm.NavigateToTitleScreen();
    }

    private static Task AnimateOpacityAsync(Control target, double from, double to, int milliseconds)
    {
        target.Opacity = from;
        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(milliseconds),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters = { new Setter(OpacityProperty, from) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters = { new Setter(OpacityProperty, to) }
                }
            }
        };

        return animation.RunAsync(target, CancellationToken.None);
    }
}
