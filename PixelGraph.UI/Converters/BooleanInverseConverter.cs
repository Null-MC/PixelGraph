using System;
using System.Globalization;
using System.Windows.Data;

namespace PixelGraph.UI.Converters
{
    [ValueConversion(typeof(object), typeof(bool))]
    internal class BooleanInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool boolValue) return Binding.DoNothing;
            return !boolValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
