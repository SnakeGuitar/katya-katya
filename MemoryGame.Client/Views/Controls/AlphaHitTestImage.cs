using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MemoryGame.Client.Views.Controls;

/// <summary>
/// An extended Image control that ignores totally transparent pixels when performing mouse hit testing.
/// This allows for precise hover boundaries on character outlines.
/// </summary>
public class AlphaHitTestImage : Image
{
    private BitmapSource? _cachedConvertedSource;
    private ImageSource? _lastSource;

    protected override HitTestResult? HitTestCore(PointHitTestParameters hitTestParameters)
    {
        var baseResult = base.HitTestCore(hitTestParameters);
        if (baseResult == null) return null;

        if (Source == null) return baseResult;

        if (Source != _lastSource)
        {
            _lastSource = Source;
            if (Source is BitmapSource bs)
            {
                _cachedConvertedSource = new FormatConvertedBitmap(bs, PixelFormats.Bgra32, null, 0);
            }
            else
            {
                _cachedConvertedSource = null;
            }
        }

        if (_cachedConvertedSource == null)
            return baseResult;

        try
        {
            BitmapSource source = _cachedConvertedSource;
            Point point = hitTestParameters.HitPoint;

            double x = 0, y = 0;
            double actualWidth = ActualWidth;
            double actualHeight = ActualHeight;
            double sourceWidth = source.PixelWidth;
            double sourceHeight = source.PixelHeight;

            if (actualWidth == 0 || actualHeight == 0) return null;

            switch (Stretch)
            {
                case Stretch.Fill:
                    x = point.X * sourceWidth / actualWidth;
                    y = point.Y * sourceHeight / actualHeight;
                    break;

                case Stretch.Uniform:
                case Stretch.UniformToFill:
                    double scaleX = actualWidth / sourceWidth;
                    double scaleY = actualHeight / sourceHeight;
                    double scale = (Stretch == Stretch.Uniform) 
                        ? Math.Min(scaleX, scaleY) 
                        : Math.Max(scaleX, scaleY);

                    double viewWidth = sourceWidth * scale;
                    double viewHeight = sourceHeight * scale;

                    double left = (actualWidth - viewWidth) / 2.0;
                    double top = (actualHeight - viewHeight) / 2.0;

                    x = (point.X - left) / scale;
                    y = (point.Y - top) / scale;
                    break;

                case Stretch.None:
                default:
                    x = point.X;
                    y = point.Y;
                    break;
            }

            int ix = (int)Math.Round(x);
            int iy = (int)Math.Round(y);

            if (ix < 0 || ix >= sourceWidth || iy < 0 || iy >= sourceHeight)
                return null;

            byte[] pixel = new byte[4];
            source.CopyPixels(new Int32Rect(ix, iy, 1, 1), pixel, 4, 0);

            if (pixel[3] < 20) 
            {
                return null;
            }
        }
        catch (Exception)
        {
        }

        return baseResult;
    }
}
