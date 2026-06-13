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

        await FadeAsync(LogoContainer, 0, 1, 3000);
        await Task.Delay(1200);

        // Fade individual elements (not the UserControl) so no container opacity
        // is ever composited against the window's transparent background.
        // BgImage uses the same GlobalBackgroundPath as the window behind it,
        // so fading it to transparent is a blur-to-crisp reveal — no darkening.
        await Task.WhenAll(
            FadeAsync(LogoContainer, 1, 0, 1000),
            FadeAsync(BgImage,       1, 0, 1000)
        );

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
