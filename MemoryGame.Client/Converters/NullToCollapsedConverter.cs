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
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        => string.IsNullOrEmpty(value?.ToString()) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
