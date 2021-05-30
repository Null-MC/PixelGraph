using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace PixelGraph.UI.Converters
{
    public class NormalMethodLabelConverter : IValueConverter
    {
        private static readonly Dictionary<NormalMapMethods, string> map = new() {
            [NormalMapMethods.Sobel3] = "Sobel-3",
            [NormalMapMethods.SobelHigh] = "Sobel-High",
            [NormalMapMethods.SobelLow] = "Sobel-Low",
            [NormalMapMethods.Variance] = "Variance",
        };


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is NormalMapMethods filter && map.TryGetValue(filter, out var label)) return label;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
