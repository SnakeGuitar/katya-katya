using System.Globalization;
using System.Windows.Data;

namespace MemoryGame.Client.Converters;

/// <summary>
/// Inverts a boolean value. Used for IsEnabled when IsLoading is true.
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b ? !b : value;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b ? !b : value;
}
