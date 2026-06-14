using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Rendering;
using KatyaKatya.Engine.Assets;
using KatyaKatya.Services.Interfaces;
using KatyaKatya.Engine.Core;
using KatyaKatya.Engine.Settings;
using KatyaKatya.ViewModels;
#if DEBUG
using KatyaKatya.Engine.Diagnostics;
#endif

namespace KatyaKatya.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel? _viewModel;
    private bool _isAnimatingNavigation;

    private readonly ISoundService? _sound;
    private readonly IGameLoop? _loop;
    private readonly IGraphicsSettingsService? _graphicsSettings;
    private readonly IVisualAssetStore? _assets;
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
        _graphicsSettings = App.Services?.GetService<IGraphicsSettingsService>();
        _assets = App.Services?.GetService<IVisualAssetStore>();
        Classes.Set("ReducedMotion", _graphicsSettings?.EnableGlassMotion == false);

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
        if (_graphicsSettings is not null && _assets is not null && _sound is not null)
            _perfOverlay.AttachServices(_graphicsSettings, _assets, _sound);
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
        // F9 toggles the global animated background — use it to isolate how much of the
        // frame budget the background costs vs. the rest of the active scene.
        else if (e.Key == Key.F9)
        {
            GlobalAnimatedBg.SetEnabled(!GlobalAnimatedBg.IsVisible);
            e.Handled = true;
        }
        // F10 toggles the Skia particle canvas to isolate particle draw/update cost.
        else if (e.Key == Key.F10)
        {
            Controls.ParticleCanvas.DiagnosticsDisabled = !Controls.ParticleCanvas.DiagnosticsDisabled;
            _loop?.Wake();
            e.Handled = true;
        }
        // F8 toggles Avalonia's native renderer overlays: FPS + dirty rects + render/layout
        // time graphs. Dirty rects show exactly which regions recompose each frame.
        else if (e.Key == Key.F8)
        {
            RendererDiagnostics.DebugOverlays = RendererDiagnostics.DebugOverlays == RendererDebugOverlays.None
                ? RendererDebugOverlays.Fps
                  | RendererDebugOverlays.DirtyRects
                  | RendererDebugOverlays.RenderTimeGraph
                  | RendererDebugOverlays.LayoutTimeGraph
                : RendererDebugOverlays.None;
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
