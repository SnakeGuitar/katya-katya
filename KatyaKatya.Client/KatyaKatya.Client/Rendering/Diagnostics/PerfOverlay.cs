using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using KatyaKatya.Rendering.Core;

namespace KatyaKatya.Rendering.Diagnostics;

/// <summary>
/// Developer-only HUD that reads live timing from <see cref="IGameLoop"/>: FPS, smoothed
/// frame time, active system count, and any per-system <see cref="IFrameDebugMetrics"/>.
/// Intended to be added to the shell under <c>#if DEBUG</c> and toggled with F12.
/// </summary>
public sealed class PerfOverlay : Border
{
    private const double RefreshSeconds = 0.25;

    private readonly TextBlock _text;
    private readonly StringBuilder _sb = new();
    private IGameLoop? _loop;
    private double _lastRefresh = double.NegativeInfinity;

    public PerfOverlay()
    {
        IsHitTestVisible = false;
        Background = new SolidColorBrush(Color.FromArgb(170, 12, 8, 16));
        BorderBrush = new SolidColorBrush(Color.FromArgb(120, 255, 105, 180));
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(8);
        Padding = new Thickness(10, 8);
        Margin = new Thickness(0, 0, 12, 12);
        HorizontalAlignment = HorizontalAlignment.Right;
        VerticalAlignment = VerticalAlignment.Bottom;
        ZIndex = 500;

        _text = new TextBlock
        {
            FontFamily = new FontFamily("Cascadia Mono, Consolas, Menlo, monospace"),
            FontSize = 12,
            Foreground = Brushes.White,
            LineHeight = 16,
        };
        Child = _text;
    }

    /// <summary>Resolve the loop and wire it in. Pass the app's singleton.</summary>
    public void Attach(IGameLoop loop)
    {
        Detach();
        _loop = loop;
        _loop.FrameCompleted += OnFrameCompleted;
        Render(_loop.LastFrame);
    }

    public void Detach()
    {
        if (_loop is not null)
            _loop.FrameCompleted -= OnFrameCompleted;
        _loop = null;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Detach();
    }

    private void OnFrameCompleted(FrameTime frame)
    {
        if (frame.TotalSeconds - _lastRefresh < RefreshSeconds)
            return;
        _lastRefresh = frame.TotalSeconds;

        if (Dispatcher.UIThread.CheckAccess())
            Render(frame);
        else
            Dispatcher.UIThread.Post(() => Render(frame));
    }

    private void Render(FrameTime frame)
    {
        if (_loop is null)
            return;

        _sb.Clear();
        _sb.Append("KATYA · PERF\n");
        _sb.Append($"FPS {frame.SmoothedFps,5:F1}  ({frame.SmoothedFrameMs,4:F1} ms)\n");
        _sb.Append($"sys {_loop.ActiveSystemCount} active / {_loop.Systems.Count}");
        if (_loop.CurrentContext is { Length: > 0 } scene)
            _sb.Append($"\nscene {scene}");

        foreach (var system in _loop.Systems)
            if (system is IFrameDebugMetrics { DebugMetrics: { Length: > 0 } metrics })
                _sb.Append('\n').Append(metrics);

        _text.Text = _sb.ToString();
    }
}
