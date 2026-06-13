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
    }

    private async void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_started || DataContext is not SplashScreenViewModel vm)
            return;

        _started = true;

        await FadeAsync(LogoContainer, 0, 1, 1100);
        await Task.Delay(1200);
        // Fade out the UserControl itself — ViewHost stays at Opacity=1 so the
        // MainWindowBorder rounded-corner clip is never composited at partial opacity.
        await FadeAsync(this, 1, 0, 1200);

        vm.NavigateToTitleScreen();
    }

    private static Task FadeAsync(Control target, double from, double to, int ms)
    {
        target.Opacity = from;
        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(ms),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame { Cue = new Cue(0d), Setters = { new Setter(OpacityProperty, from) } },
                new KeyFrame { Cue = new Cue(1d), Setters = { new Setter(OpacityProperty, to) } }
            }
        };
        return animation.RunAsync(target, CancellationToken.None);
    }
}
