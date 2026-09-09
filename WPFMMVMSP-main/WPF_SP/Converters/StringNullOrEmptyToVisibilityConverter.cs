using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WPF_SP.Converters;

public class StringNullOrEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isEmpty = string.IsNullOrWhiteSpace(value as string);
        var invert = parameter is string p && p == "Invert";
        if (invert) isEmpty = !isEmpty;
        return isEmpty ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
