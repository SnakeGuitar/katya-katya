using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MemoryGame.Client.Converters;

/// <summary>
/// Converts a boolean to Visibility. True = Visible, False = Collapsed.
/// Pass ConverterParameter="invert" to reverse the logic.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var b = value is bool v && v;
        var invert = parameter is string s && s.Equals("invert", StringComparison.OrdinalIgnoreCase);
        if (invert) b = !b;
        return b ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
