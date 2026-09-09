using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WPF_SP.Converters;

public class BoolToStrikethroughConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? TextDecorations.Strikethrough : null!;
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
