using System;
using System.Globalization;
using System.Windows.Data;

namespace PixelGraph.UI.Converters;

[ValueConversion(typeof(object), typeof(bool))]
internal class EmptyToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return true;
        if (value is string _str && string.IsNullOrWhiteSpace(_str)) return true;
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}

//[ValueConversion(typeof(object), typeof(Visibility))]
//internal class NotNullToBooleanConverter : IValueConverter
//{
//    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        return value != null ? Visibility.Collapsed : Visibility.Visible;
//    }

//    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        return Binding.DoNothing;
//    }
//}