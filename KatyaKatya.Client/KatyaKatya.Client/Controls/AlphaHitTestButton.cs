using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Rendering;
using SkiaSharp;

namespace KatyaKatya.Controls;

/// <summary>
/// Button that only receives pointer hits on opaque pixels of its bound image asset.
/// </summary>
public class AlphaHitTestButton : Button, ICustomHitTest
{
    public static readonly StyledProperty<string?> AlphaMaskSourceProperty =
        AvaloniaProperty.Register<AlphaHitTestButton, string?>(nameof(AlphaMaskSource));

    private string? _loadedSource;
    private SKBitmap? _mask;

    public string? AlphaMaskSource
    {
        get => GetValue(AlphaMaskSourceProperty);
        set => SetValue(AlphaMaskSourceProperty, value);
    }

    public bool HitTest(Point point)
    {
        if (point.X < 0 || point.Y < 0 || point.X > Bounds.Width || point.Y > Bounds.Height)
            return false;

        var mask = GetMask();
        if (mask is null || Bounds.Width <= 0 || Bounds.Height <= 0)
            return true;

        var sourceWidth = mask.Width;
        var sourceHeight = mask.Height;
        var scale = Math.Min(Bounds.Width / sourceWidth, Bounds.Height / sourceHeight);
        var viewWidth = sourceWidth * scale;
        var viewHeight = sourceHeight * scale;
        var left = (Bounds.Width - viewWidth) / 2.0;
        var top = (Bounds.Height - viewHeight) / 2.0;

        var x = (point.X - left) / scale;
        var y = (point.Y - top) / scale;
        var ix = (int)Math.Floor(x);
        var iy = (int)Math.Floor(y);

        if (ix < 0 || iy < 0 || ix >= sourceWidth || iy >= sourceHeight)
            return false;

        return mask.GetPixel(ix, iy).Alpha >= 4;
    }

    private SKBitmap? GetMask()
    {
        var source = AlphaMaskSource;
        if (string.IsNullOrWhiteSpace(source))
            return null;

        if (_mask is not null && _loadedSource == source)
            return _mask;

        _mask?.Dispose();
        _mask = null;
        _loadedSource = source;

        try
        {
            using var stream = AssetLoader.Open(new Uri(source));
            _mask = SKBitmap.Decode(stream);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AlphaHitTestButton] Failed to load mask {source}: {ex.Message}");
        }

        return _mask;
    }
}
