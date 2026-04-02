using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MemoryGame.Client.Behaviors;

/// <summary>
/// Provides attached behaviors to enable interactive zoom and panning on any <see cref="ScrollViewer"/>.
/// Adds support for zooming via the mouse wheel and panning the content by clicking and dragging
/// with the left mouse button.
/// </summary>
public static class ZoomPanBehavior
{
    /// <summary>
    /// Attached property that enables or disables zoom and panning interaction on an element.
    /// When set to <c>true</c> on a <see cref="ScrollViewer"/>, the event handlers are automatically hooked.
    /// </summary>
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled", typeof(bool), typeof(ZoomPanBehavior), new PropertyMetadata(false, OnIsEnabledChanged));

    /// <summary>
    /// Gets the value indicating whether the behavior is enabled.
    /// </summary>
    /// <param name="obj">The dependency object.</param>
    /// <returns><c>true</c> if enabled; otherwise, <c>false</c>.</returns>
    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
    
    /// <summary>
    /// Sets whether the interactive zoom and panning behavior is enabled for the targeted control.
    /// </summary>
    /// <param name="obj">The dependency object.</param>
    /// <param name="value">The value to set.</param>
    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    /// <summary>
    /// Internal attached property to track the last cursor position during a drag operation.
    /// </summary>
    private static readonly DependencyProperty LastMousePositionProperty = DependencyProperty.RegisterAttached(
        "LastMousePosition", typeof(Point?), typeof(ZoomPanBehavior), new PropertyMetadata(null));

    /// <summary>
    /// Internal attached property to track the current target zoom level.
    /// </summary>
    private static readonly DependencyProperty TargetZoomProperty = DependencyProperty.RegisterAttached(
        "TargetZoom", typeof(double), typeof(ZoomPanBehavior), new PropertyMetadata(1.0));

    /// <summary>
    /// Attached property designed to be bound to an external observable property.
    /// Whenever the value of this property changes (e.g. selecting a different component or image),
    /// the system will catch the event and smoothly reset the zoom back to its original 1.0 view.
    /// </summary>
    public static readonly DependencyProperty ResetZoomTriggerProperty = DependencyProperty.RegisterAttached(
        "ResetZoomTrigger", typeof(object), typeof(ZoomPanBehavior), new PropertyMetadata(null, OnResetZoomTriggerChanged));

    /// <summary>
    /// Sets the trigger used dynamically as a flag to restore the zoom level to normalcy.
    /// </summary>
    /// <param name="obj">The dependency object.</param>
    /// <param name="value">The value to set.</param>
    public static void SetResetZoomTrigger(DependencyObject obj, object value) => obj.SetValue(ResetZoomTriggerProperty, value);

    /// <summary>
    /// Handles the IsEnabled property change event. Connects or disconnects the mouse interaction handlers.
    /// </summary>
    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollViewer scrollViewer)
        {
            if ((bool)e.NewValue)
            {
                scrollViewer.PreviewMouseWheel += OnMouseWheel;
                scrollViewer.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
                scrollViewer.PreviewMouseMove += OnMouseMove;
                scrollViewer.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
            }
            else
            {
                scrollViewer.PreviewMouseWheel -= OnMouseWheel;
                scrollViewer.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
                scrollViewer.PreviewMouseMove -= OnMouseMove;
                scrollViewer.PreviewMouseLeftButtonUp -= OnMouseLeftButtonUp;
            }
        }
    }

    /// <summary>
    /// Handles the ResetZoomTrigger property change event. Resets the zoom when the bound object changes.
    /// </summary>
    private static void OnResetZoomTriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollViewer scrollViewer && GetIsEnabled(scrollViewer))
        {
            ResetZoom(scrollViewer);
        }
    }

    /// <summary>
    /// Resets the zoom back to 1.0 by animating the underlying ScaleTransform.
    /// </summary>
    private static void ResetZoom(ScrollViewer scrollViewer)
    {
        scrollViewer.SetValue(TargetZoomProperty, 1.0);
        AnimateZoom(scrollViewer, 1.0);
    }

    /// <summary>
    /// Inspects the content of the ScrollViewer to find or inject a ScaleTransform into its LayoutTransform.
    /// </summary>
    private static ScaleTransform? GetScaleTransform(ScrollViewer scrollViewer)
    {
        if (scrollViewer.Content is FrameworkElement content)
        {
            if (content.LayoutTransform is ScaleTransform existingScale)
                return existingScale;

            if (content.LayoutTransform is TransformGroup group)
            {
                var scale = group.Children.OfType<ScaleTransform>().FirstOrDefault();
                if (scale != null) return scale;
            }

            var newScale = new ScaleTransform(1, 1);
            content.LayoutTransform = newScale;
            return newScale;
        }
        return null;
    }

    /// <summary>
    /// Intercepts the mouse wheel scroll to compute the target scale and applies an animated zoom.
    /// </summary>
    private static void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer) return;

        double targetZoom = (double)scrollViewer.GetValue(TargetZoomProperty);
        double zoomFactor = e.Delta > 0 ? 1.25 : 1 / 1.25;
        
        targetZoom *= zoomFactor;

        if (targetZoom < 1.0) targetZoom = 1.0;
        if (targetZoom > 4.0) targetZoom = 4.0;

        scrollViewer.SetValue(TargetZoomProperty, targetZoom);
        AnimateZoom(scrollViewer, targetZoom);
        e.Handled = true;
    }

    /// <summary>
    /// Begins a fluid double animation on the content's ScaleX and ScaleY properties.
    /// </summary>
    private static void AnimateZoom(ScrollViewer scrollViewer, double target)
    {
        var scaleTransform = GetScaleTransform(scrollViewer);
        if (scaleTransform == null) return;

        var anim = new DoubleAnimation(target, TimeSpan.FromMilliseconds(200))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
    }

    /// <summary>
    /// Occurs when the left mouse button is pressed. Captures the mouse over the inner content to start panning.
    /// </summary>
    private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer) return;
        if (e.OriginalSource is DependencyObject source && IsChildOfButton(source)) return;

        var content = scrollViewer.Content as UIElement;
        if (content == null) return;

        scrollViewer.SetValue(LastMousePositionProperty, e.GetPosition(scrollViewer));
        content.CaptureMouse();
    }

    /// <summary>
    /// Occurs when the mouse cursor moves. Drags the scroll offsets if the mouse has been properly captured.
    /// </summary>
    private static void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer) return;
        var content = scrollViewer.Content as UIElement;
        if (content == null || !content.IsMouseCaptured) return;

        var lastPosition = (Point?)scrollViewer.GetValue(LastMousePositionProperty);
        if (lastPosition.HasValue)
        {
            Point currentPosition = e.GetPosition(scrollViewer);
            double deltaX = currentPosition.X - lastPosition.Value.X;
            double deltaY = currentPosition.Y - lastPosition.Value.Y;

            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - deltaX);
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - deltaY);

            scrollViewer.SetValue(LastMousePositionProperty, currentPosition);
        }
    }

    /// <summary>
    /// Occurs when the left mouse button is released. Releases the mouse capture and ends the pan drag event.
    /// </summary>
    private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer) return;
        var content = scrollViewer.Content as UIElement;
        
        content?.ReleaseMouseCapture();
        scrollViewer.SetValue(LastMousePositionProperty, null);
    }

    /// <summary>
    /// Helper method to determine if the clicked element sits inside a button's visual tree, avoiding panning interference.
    /// </summary>
    private static bool IsChildOfButton(DependencyObject obj)
    {
        while (obj != null)
        {
            if (obj is System.Windows.Controls.Primitives.ButtonBase) return true;
            obj = VisualTreeHelper.GetParent(obj);
        }
        return false;
    }
}
