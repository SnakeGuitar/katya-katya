using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace KatyaKatya.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

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
}
