using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using KatyaKatya.Engine.Core;
using KatyaKatya.Engine.Effects;
using KatyaKatya.Engine.Settings;

namespace KatyaKatya.Controls;

/// <summary>
/// Skia-backed romantic background: mist, clouds, bokeh, spotlight and vignette.
/// Keeps the original public control surface used by XAML and debug hotkeys.
/// </summary>
public sealed class AnimatedBackground : Control, IFrameUpdatable, IFrameDebugMetrics
{
    private readonly RomanticBackgroundRenderer _renderer = new();
    private IGameLoop? _loop;
    private IGraphicsSettingsService? _graphicsSettings;
    private TopLevel? _topLevel;
    private Point _targetPointer;
    private bool _enabled = true;

    public AnimatedBackground()
    {
        IsHitTestVisible = false;
        ClipToBounds = true;
    }

    public bool IsActive =>
        _enabled
        && (_graphicsSettings?.EnableAnimatedBackground ?? true)
        && IsEffectivelyVisible;

    string? IFrameDebugMetrics.DebugMetrics =>
        $"bg skia layers:{_renderer.LayerCount}";

    public void SetEnabled(bool on)
    {
        _enabled = on;
        IsVisible = on;
        if (on)
            _loop?.Register(this);
        else
            _loop?.Unregister(this);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _topLevel = TopLevel.GetTopLevel(this);
        if (_topLevel is not null)
            _topLevel.PointerMoved += OnPointerMoved;

        _loop ??= App.Services?.GetService<IGameLoop>();
        _graphicsSettings ??= App.Services?.GetService<IGraphicsSettingsService>();
        _loop?.Register(this);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (_topLevel is not null)
        {
            _topLevel.PointerMoved -= OnPointerMoved;
            _topLevel = null;
        }

        _loop?.Unregister(this);
    }

    public void Tick(in FrameTime time)
    {
        if (!IsActive)
            return;

        _renderer.Update(time.DeltaSeconds, Bounds.Size, _targetPointer);
        InvalidateVisual();
    }

    public override void Render(Avalonia.Media.DrawingContext context)
    {
        base.Render(context);

        if (!IsActive || Bounds.Width <= 1 || Bounds.Height <= 1)
            return;

        var density = _graphicsSettings?.BackgroundDensity ?? 1.0f;
        context.Custom(_renderer.CreateDrawOperation(new Rect(Bounds.Size), density));
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        _targetPointer = e.GetPosition(this);
    }
}

