using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace PixelGraph.UI.Converters
{
    public class NormalFilterLabelConverter : IValueConverter
    {
        private static readonly Dictionary<NormalMapFilters, string> map = new Dictionary<NormalMapFilters, string> {
            [NormalMapFilters.Sobel3] = "Sobel-3",
            [NormalMapFilters.SobelHigh] = "Sobel-High",
            [NormalMapFilters.SobelLow] = "Sobel-Low",
            //[NormalMapFilters.Spline] = "Spline",
        };


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is NormalMapFilters filter && map.TryGetValue(filter, out var label)) return label;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
