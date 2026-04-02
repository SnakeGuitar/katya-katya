using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;
using MemoryGame.Client.Services;
using MemoryGame.Client.ViewModels;

namespace MemoryGame.Client;

/// <summary>
/// Single-window shell. All navigation happens via ContentControl + DataTemplates.
/// Transitions between views are animated here so individual views don't need their own fade-in logic.
/// </summary>
public partial class MainWindow : Window
{
    private const int FadeOutMs = 120;
    private const int FadeInMs  = 180;

    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is MainWindowViewModel old)
            ((INotifyPropertyChanged)old.Navigation).PropertyChanged -= OnNavigationChanged;

        if (e.NewValue is MainWindowViewModel next)
            ((INotifyPropertyChanged)next.Navigation).PropertyChanged += OnNavigationChanged;
    }

    private async void OnNavigationChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(INavigationService.CurrentViewModel)) return;
        if (sender is not INavigationService nav) return;

        var next    = nav.CurrentViewModel;
        var animate = nav.IsAnimatedTransition;

        if (animate && PageContent.Content is not null)
            await AnimateOpacityAsync(0, FadeOutMs);

        PageContent.Content  = next;
        PageContent.Opacity  = 1;

        if (animate)
            await AnimateOpacityAsync(1, FadeInMs);
    }

    private Task AnimateOpacityAsync(double to, int durationMs)
    {
        var tcs       = new TaskCompletionSource();
        var animation = new DoubleAnimation(to, TimeSpan.FromMilliseconds(durationMs));
        animation.Completed += (_, _) => tcs.TrySetResult();
        PageContent.BeginAnimation(OpacityProperty, animation);
        return tcs.Task;
    }
}
