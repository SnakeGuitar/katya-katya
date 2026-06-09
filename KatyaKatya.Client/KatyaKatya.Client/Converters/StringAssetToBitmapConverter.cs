using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace KatyaKatya.Converters;

/// <summary>
/// Converts a string asset URI (e.g. avares://KatyaKatya/Resources/...) to a Bitmap.
/// </summary>
public class StringAssetToBitmapConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string path && !string.IsNullOrWhiteSpace(path))
        {
            try
            {
                if (File.Exists(path))
                {
                    using var file = File.OpenRead(path);
                    return new Bitmap(file);
                }

                var uri = new Uri(path);
                using var stream = AssetLoader.Open(uri);
                return new Bitmap(stream);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StringAssetToBitmapConverter] Error loading bitmap: {ex.Message}");
                return null;
            }
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
