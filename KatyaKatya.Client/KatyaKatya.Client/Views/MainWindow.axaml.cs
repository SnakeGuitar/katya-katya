using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using KatyaKatya.Services.Interfaces;
using KatyaKatya.Rendering.Core;
using KatyaKatya.ViewModels;
#if DEBUG
using KatyaKatya.Rendering.Diagnostics;
#endif

namespace KatyaKatya.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel? _viewModel;
    private bool _isAnimatingNavigation;

    private readonly ISoundService? _sound;
    private readonly IGameLoop? _loop;
    private Button? _lastHoveredButton;
#if DEBUG
    private PerfOverlay? _perfOverlay;
#endif

    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;

        _sound = App.Services?.GetService<ISoundService>();
        _loop = App.Services?.GetService<IGameLoop>();

#if DEBUG
        InitPerfOverlay();
#endif

        // Global UI sound effects: hover tick + click pop on every button.
        // PointerMoved bubbles to the window, so we detect the button under the
        // cursor and fire the tick once when it changes. Button.Click also bubbles.
        AddHandler(PointerMovedEvent, OnGlobalPointerMoved, RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(Button.ClickEvent, OnGlobalButtonClick, RoutingStrategies.Bubble, handledEventsToo: true);

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

    private void OnGlobalPointerMoved(object? sender, PointerEventArgs e)
    {
        var button = (e.Source as Visual)?.FindAncestorOfType<Button>(includeSelf: true);

        if (ReferenceEquals(button, _lastHoveredButton))
            return;

        _lastHoveredButton = button;

        if (button is not null && button.IsEffectivelyEnabled)
            _sound?.PlayHover();
    }

    private void OnGlobalButtonClick(object? sender, RoutedEventArgs e)
        => _sound?.PlayClick();

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

#if DEBUG
    private void InitPerfOverlay()
    {
        if (_loop is null || Content is not Grid root)
            return;

        _perfOverlay = new PerfOverlay { IsVisible = false };
        _perfOverlay.Attach(_loop);
        root.Children.Add(_perfOverlay);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        // F12 toggles the developer performance overlay.
        if (e.Key == Key.F12 && _perfOverlay is not null)
        {
            _perfOverlay.IsVisible = !_perfOverlay.IsVisible;
            e.Handled = true;
        }

        base.OnKeyDown(e);
    }
#endif

    private async void OnAnimatedNavigationRequested(ObservableObject? nextViewModel)
    {
        if (_loop is not null)
            _loop.CurrentContext = nextViewModel?.GetType().Name;

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
