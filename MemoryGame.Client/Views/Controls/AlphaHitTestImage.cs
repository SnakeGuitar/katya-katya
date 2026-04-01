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
    protected override HitTestResult? HitTestCore(PointHitTestParameters hitTestParameters)
    {
        var baseResult = base.HitTestCore(hitTestParameters);
        if (baseResult == null) return null;

        if (Source is not BitmapSource source)
            return baseResult;

        try
        {
            int x = (int)(hitTestParameters.HitPoint.X * source.PixelWidth / ActualWidth);
            int y = (int)(hitTestParameters.HitPoint.Y * source.PixelHeight / ActualHeight);

            if (x < 0 || x >= source.PixelWidth || y < 0 || y >= source.PixelHeight)
                return null;

            byte[] pixel = new byte[4];
            
            source.CopyPixels(new Int32Rect(x, y, 1, 1), pixel, 4, 0);

            if (pixel[3] < 10)
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
