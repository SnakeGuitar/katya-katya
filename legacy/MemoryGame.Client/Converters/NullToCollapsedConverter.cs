using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MemoryGame.Client.Converters;

/// <summary>
/// Returns Visible when the value is non-null/non-empty, Collapsed otherwise.
/// Used for showing error messages only when they exist.
/// </summary>
public class NullToCollapsedConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var hasValue = !string.IsNullOrEmpty(value?.ToString());
        var invert = parameter is string s && s.Equals("invert", StringComparison.OrdinalIgnoreCase);
        if (invert) hasValue = !hasValue;
        return hasValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
