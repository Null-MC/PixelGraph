using System;
using System.Globalization;
using System.Windows.Data;

namespace PixelGraph.UI.Converters;

[ValueConversion(typeof(decimal?), typeof(double?))]
public class DecimalToDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return null;
        return decimal.ToDouble((decimal)value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return null;
        return decimal.Round(new decimal((double)value), 4);
    }
}