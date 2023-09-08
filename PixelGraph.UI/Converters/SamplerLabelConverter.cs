using PixelGraph.Common.Samplers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace PixelGraph.UI.Converters;

internal class SamplerLabelConverter : IValueConverter
{
    private static readonly Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
        //[Sampler.Point] = "Point",
        [Samplers.Nearest] = "Nearest",
        [Samplers.Bilinear] = "Bilinear",
        [Samplers.Average] = "Average",
        //...
    };


    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue && map.TryGetValue(stringValue, out var label)) return label;
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}