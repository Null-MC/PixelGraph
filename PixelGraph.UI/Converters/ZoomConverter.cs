using PixelGraph.UI.Internal;
using System;
using System.Globalization;
using System.Windows.Data;

namespace PixelGraph.UI.Converters
{
    [ValueConversion(typeof(string), typeof(double))]
    internal class ZoomConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
                return ZoomHelper.Parse(stringValue);

            return 1d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
