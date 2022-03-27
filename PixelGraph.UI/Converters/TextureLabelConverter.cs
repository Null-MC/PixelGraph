using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace PixelGraph.UI.Converters
{
    internal class TextureLabelConverter : IValueConverter
    {
        private static readonly Dictionary<string, string> map = new(StringComparer.InvariantCultureIgnoreCase) {
            [TextureTags.Opacity] = "Opacity",
            [TextureTags.Color] = "Color",
            [TextureTags.Height] = "Height",
            [TextureTags.Occlusion] = "Occlusion",
            [TextureTags.Normal] = "Normal",
            [TextureTags.Specular] = "Specular",
            [TextureTags.Smooth] = "Smooth",
            [TextureTags.Rough] = "Rough",
            [TextureTags.Metal] = "Metal",
            [TextureTags.HCM] = "HCM",
            [TextureTags.Porosity] = "Porosity",
            [TextureTags.SubSurfaceScattering] = "SubSurfaceScattering",
            [TextureTags.Emissive] = "Emissive",
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
}
