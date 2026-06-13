using System;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Rendering;

namespace KatyaKatya.Controls;

/// <summary>
/// An <see cref="Image"/> that only receives pointer hits on non-transparent pixels of its
/// <see cref="Image.Source"/>, so hover/click boundaries follow the character silhouette.
/// Mirrors the WPF <c>AlphaHitTestImage</c>: a single visual with no template, so nothing
/// (no wrapping button, no transparent background) can steal hits across the full rectangle.
/// </summary>
public class AlphaHitTestImage : Image, ICustomHitTest
{
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<AlphaHitTestImage, ICommand?>(nameof(Command));

    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<AlphaHitTestImage, object?>(nameof(CommandParameter));

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    private object? _maskSource;
    private byte[]? _alpha;
    private int _maskWidth;
    private int _maskHeight;

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (e.InitialPressMouseButton != MouseButton.Left)
            return;

        var command = Command;
        if (command is not null && command.CanExecute(CommandParameter))
            command.Execute(CommandParameter);
    }

    public bool HitTest(Point point)
    {
        var bounds = Bounds;
        if (point.X < 0 || point.Y < 0 || point.X > bounds.Width || point.Y > bounds.Height)
            return false;

        EnsureMask();
        if (_alpha is null || _maskWidth == 0 || _maskHeight == 0 || bounds.Width <= 0 || bounds.Height <= 0)
            return true;

        // Map the point through the Stretch="Uniform" layout the same way Avalonia renders it.
        var scale = Math.Min(bounds.Width / _maskWidth, bounds.Height / _maskHeight);
        var viewWidth = _maskWidth * scale;
        var viewHeight = _maskHeight * scale;
        var left = (bounds.Width - viewWidth) / 2.0;
        var top = (bounds.Height - viewHeight) / 2.0;

        var ix = (int)Math.Floor((point.X - left) / scale);
        var iy = (int)Math.Floor((point.Y - top) / scale);

        if (ix < 0 || iy < 0 || ix >= _maskWidth || iy >= _maskHeight)
            return false;

        return _alpha[iy * _maskWidth + ix] >= 20;
    }

    private void EnsureMask()
    {
        var source = Source;
        if (ReferenceEquals(source, _maskSource))
            return;

        _maskSource = source;
        _alpha = null;
        _maskWidth = _maskHeight = 0;

        if (source is not Bitmap bitmap)
            return;

        try
        {
            var size = bitmap.PixelSize;
            var stride = size.Width * 4;
            var buffer = new byte[stride * size.Height];

            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                bitmap.CopyPixels(new PixelRect(size), handle.AddrOfPinnedObject(), buffer.Length, stride);
            }
            finally
            {
                handle.Free();
            }

            // Keep only the alpha channel (BGRA → byte index 3) to minimize retained memory.
            var alpha = new byte[size.Width * size.Height];
            for (var i = 0; i < alpha.Length; i++)
                alpha[i] = buffer[i * 4 + 3];

            _alpha = alpha;
            _maskWidth = size.Width;
            _maskHeight = size.Height;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AlphaHitTestImage] Failed to read alpha mask: {ex.Message}");
        }
    }
}
