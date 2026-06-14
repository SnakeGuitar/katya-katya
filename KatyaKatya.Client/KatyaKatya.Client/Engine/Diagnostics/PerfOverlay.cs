using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using KatyaKatya.Engine.Assets;
using KatyaKatya.Engine.Core;
using KatyaKatya.Engine.Settings;
using KatyaKatya.Services.Interfaces;

namespace KatyaKatya.Engine.Diagnostics;

/// <summary>
/// Developer-only HUD that reads live timing from <see cref="IGameLoop"/> and the
/// lightweight engine services involved in the current frame budget.
/// </summary>
public sealed class PerfOverlay : Border
{
    private const double RefreshSeconds = 0.25;

    private readonly TextBlock _text;
    private readonly StringBuilder _sb = new();
    private IGameLoop? _loop;
    private IGraphicsSettingsService? _graphicsSettings;
    private IVisualAssetStore? _assets;
    private ISoundService? _sound;
    private double _lastRefresh = double.NegativeInfinity;
    private int _slowFrames;

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

    public void Attach(IGameLoop loop)
    {
        Detach();
        _loop = loop;
        _loop.FrameCompleted += OnFrameCompleted;
        _loop.SlowFrame += OnSlowFrame;
        Render(_loop.LastFrame);
    }

    public void AttachServices(
        IGraphicsSettingsService graphicsSettings,
        IVisualAssetStore assets,
        ISoundService sound)
    {
        _graphicsSettings = graphicsSettings;
        _assets = assets;
        _sound = sound;
    }

    public void Detach()
    {
        if (_loop is not null)
        {
            _loop.FrameCompleted -= OnFrameCompleted;
            _loop.SlowFrame -= OnSlowFrame;
        }

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

    private void OnSlowFrame(FrameTime frame) => _slowFrames++;

    private void Render(FrameTime frame)
    {
        if (_loop is null)
            return;

        _sb.Clear();
        _sb.Append("KATYA PERF\n");
        _sb.Append($"FPS {frame.SmoothedFps,5:F1}  ({frame.SmoothedFrameMs,4:F1} ms)\n");
        _sb.Append($"sys {_loop.ActiveSystemCount} active / {_loop.Systems.Count}");
        _sb.Append($"\nslow {_slowFrames}");

        if (_graphicsSettings is not null)
        {
            _sb.Append($"\npreset {_graphicsSettings.Preset}");
            _sb.Append($" bg:{_graphicsSettings.EnableAnimatedBackground}");
            _sb.Append($" fx:{_graphicsSettings.EnableParticles}");
            _sb.Append($" glass:{_graphicsSettings.EnableGlassMotion}");
        }

        if (_loop.CurrentContext is { Length: > 0 } scene)
            _sb.Append($"\nscene {scene}");

        foreach (var system in _loop.Systems)
            if (system is IFrameDebugMetrics { DebugMetrics: { Length: > 0 } metrics })
                _sb.Append('\n').Append(metrics);

        if (_assets is not null)
            _sb.Append('\n').Append(_assets.DebugMetrics);
        if (_sound is not null)
            _sb.Append('\n').Append(_sound.DebugMetrics);

        _text.Text = _sb.ToString();
    }
}
