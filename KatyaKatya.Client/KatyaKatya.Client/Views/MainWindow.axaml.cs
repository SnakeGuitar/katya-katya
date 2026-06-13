using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using KatyaKatya.ViewModels;

namespace KatyaKatya.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel? _viewModel;
    private bool _isAnimatingNavigation;

    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;

        DragArea.PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                BeginMoveDrag(e);
        };

        ResizeN.PointerPressed  += (_, e) => BeginResizeDrag(WindowEdge.North, e);
        ResizeS.PointerPressed  += (_, e) => BeginResizeDrag(WindowEdge.South, e);
        ResizeW.PointerPressed  += (_, e) => BeginResizeDrag(WindowEdge.West,  e);
        ResizeE.PointerPressed  += (_, e) => BeginResizeDrag(WindowEdge.East,  e);
        ResizeNW.PointerPressed += (_, e) => BeginResizeDrag(WindowEdge.NorthWest, e);
        ResizeNE.PointerPressed += (_, e) => BeginResizeDrag(WindowEdge.NorthEast, e);
        ResizeSW.PointerPressed += (_, e) => BeginResizeDrag(WindowEdge.SouthWest, e);
        ResizeSE.PointerPressed += (_, e) => BeginResizeDrag(WindowEdge.SouthEast, e);
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e)
        => Close();

    private void OnMinimizeClicked(object? sender, RoutedEventArgs e)
        => WindowState = WindowState.Minimized;

    private void OnMaximizeClicked(object? sender, RoutedEventArgs e)
        => WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel is not null)
            _viewModel.AnimatedNavigationRequested -= OnAnimatedNavigationRequested;

        _viewModel = DataContext as MainWindowViewModel;

        if (_viewModel is not null)
            _viewModel.AnimatedNavigationRequested += OnAnimatedNavigationRequested;
    }

    private async void OnAnimatedNavigationRequested(ObservableObject? nextViewModel)
    {
        if (_viewModel is null)
            return;

        if (_isAnimatingNavigation)
        {
            _viewModel.CommitAnimatedNavigation(nextViewModel);
            ViewHost.Opacity = 1;
            return;
        }

        _isAnimatingNavigation = true;
        try
        {
            await AnimateOpacityAsync(FadeOverlay, 0, 1, 180);
            _viewModel.CommitAnimatedNavigation(nextViewModel);
            await AnimateOpacityAsync(FadeOverlay, 1, 0, 240);
        }
        finally
        {
            _isAnimatingNavigation = false;
            FadeOverlay.Opacity = 0;
        }
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
