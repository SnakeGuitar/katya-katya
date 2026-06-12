using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Transformation;

namespace KatyaKatya.Behaviors;

/// <summary>
/// Attached behavior providing wheel zoom (smoothly animated) and drag panning
/// on any control. Panning is only available while zoomed in and is clamped so
/// the content can never be dragged fully out of view.
/// </summary>
public sealed class ZoomPanBehavior
{
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<ZoomPanBehavior, Control, bool>("IsEnabled");

    public static readonly AttachedProperty<object?> ResetZoomTriggerProperty =
        AvaloniaProperty.RegisterAttached<ZoomPanBehavior, Control, object?>("ResetZoomTrigger");

    private static readonly AttachedProperty<Point?> LastPointerPositionProperty =
        AvaloniaProperty.RegisterAttached<ZoomPanBehavior, Control, Point?>("LastPointerPosition");

    private static readonly AttachedProperty<double> ZoomProperty =
        AvaloniaProperty.RegisterAttached<ZoomPanBehavior, Control, double>("Zoom", 1.0);

    private static readonly AttachedProperty<Vector> PanProperty =
        AvaloniaProperty.RegisterAttached<ZoomPanBehavior, Control, Vector>("Pan");

    private static readonly AttachedProperty<TransformOperationsTransition?> ZoomTransitionProperty =
        AvaloniaProperty.RegisterAttached<ZoomPanBehavior, Control, TransformOperationsTransition?>("ZoomTransition");

    static ZoomPanBehavior()
    {
        IsEnabledProperty.Changed.AddClassHandler<Control>(OnIsEnabledChanged);
        ResetZoomTriggerProperty.Changed.AddClassHandler<Control>((control, _) => Reset(control));
    }

    public static bool GetIsEnabled(Control control) => control.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(Control control, bool value) => control.SetValue(IsEnabledProperty, value);

    public static object? GetResetZoomTrigger(Control control) => control.GetValue(ResetZoomTriggerProperty);
    public static void SetResetZoomTrigger(Control control, object? value) => control.SetValue(ResetZoomTriggerProperty, value);

    private static void OnIsEnabledChanged(Control control, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue is true)
        {
            control.PointerWheelChanged += OnPointerWheelChanged;
            control.PointerPressed += OnPointerPressed;
            control.PointerMoved += OnPointerMoved;
            control.PointerReleased += OnPointerReleased;

            var transition = new TransformOperationsTransition
            {
                Property = Visual.RenderTransformProperty,
                Duration = TimeSpan.FromMilliseconds(200),
                Easing = new CubicEaseOut(),
            };
            control.SetValue(ZoomTransitionProperty, transition);
            control.Transitions ??= [];
            control.Transitions.Add(transition);

            Reset(control);
        }
        else
        {
            control.PointerWheelChanged -= OnPointerWheelChanged;
            control.PointerPressed -= OnPointerPressed;
            control.PointerMoved -= OnPointerMoved;
            control.PointerReleased -= OnPointerReleased;
            control.SetValue(LastPointerPositionProperty, null);

            if (control.GetValue(ZoomTransitionProperty) is { } transition)
            {
                control.Transitions?.Remove(transition);
                control.SetValue(ZoomTransitionProperty, null);
            }
        }
    }

    private static void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (sender is not Control control)
            return;

        var zoom = control.GetValue(ZoomProperty);
        zoom *= e.Delta.Y > 0 ? 1.25 : 1 / 1.25;
        zoom = Math.Clamp(zoom, 1.0, 4.0);

        control.SetValue(ZoomProperty, zoom);
        control.SetValue(PanProperty, ClampPan(control, control.GetValue(PanProperty), zoom));
        ApplyTransform(control);
        e.Handled = true;
    }

    private static void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control control)
            return;

        if (!e.GetCurrentPoint(control).Properties.IsLeftButtonPressed)
            return;

        // Nothing to pan at 1:1 — let clicks pass through untouched
        if (control.GetValue(ZoomProperty) <= 1.0)
            return;

        control.SetValue(LastPointerPositionProperty, e.GetPosition(control));
        SetTransitionEnabled(control, false); // Drags must track the pointer 1:1
        e.Pointer.Capture(control);
        e.Handled = true;
    }

    private static void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (sender is not Control control)
            return;

        var last = control.GetValue(LastPointerPositionProperty);
        if (last is null)
            return;

        var current = e.GetPosition(control);
        var delta = current - last.Value;
        var zoom = control.GetValue(ZoomProperty);
        var pan = control.GetValue(PanProperty) + new Vector(delta.X, delta.Y);

        control.SetValue(PanProperty, ClampPan(control, pan, zoom));
        control.SetValue(LastPointerPositionProperty, current);
        ApplyTransform(control);
        e.Handled = true;
    }

    private static void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is not Control control)
            return;

        if (control.GetValue(LastPointerPositionProperty) is null)
            return;

        control.SetValue(LastPointerPositionProperty, null);
        SetTransitionEnabled(control, true);
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private static void Reset(Control control)
    {
        control.SetValue(ZoomProperty, 1.0);
        control.SetValue(PanProperty, default(Vector));
        ApplyTransform(control);
    }

    /// <summary>Keeps at least the visible area covered: |pan| ≤ size·(zoom−1)/2 per axis.</summary>
    private static Vector ClampPan(Control control, Vector pan, double zoom)
    {
        var maxX = Math.Max(0, control.Bounds.Width * (zoom - 1) / 2);
        var maxY = Math.Max(0, control.Bounds.Height * (zoom - 1) / 2);
        return new Vector(
            Math.Clamp(pan.X, -maxX, maxX),
            Math.Clamp(pan.Y, -maxY, maxY));
    }

    private static void SetTransitionEnabled(Control control, bool enabled)
    {
        if (control.GetValue(ZoomTransitionProperty) is not { } transition || control.Transitions is null)
            return;

        if (enabled && !control.Transitions.Contains(transition))
            control.Transitions.Add(transition);
        else if (!enabled)
            control.Transitions.Remove(transition);
    }

    private static void ApplyTransform(Control control)
    {
        var zoom = control.GetValue(ZoomProperty);
        var pan = control.GetValue(PanProperty);

        control.RenderTransformOrigin = RelativePoint.Center;
        // Rightmost operation applies first: scale around the centre, then pan in screen pixels
        control.RenderTransform = TransformOperations.Parse(
            FormattableString.Invariant($"translate({pan.X}px, {pan.Y}px) scale({zoom})"));
    }
}
