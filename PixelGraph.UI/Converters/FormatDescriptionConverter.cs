using PixelGraph.Common.Encoding;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace PixelGraph.UI.Converters
{
    internal class FormatDescriptionConverter : IValueConverter
    {
        public const string NoneDescription = "No channel mappings. Do not use this without manually specifying encoding channels.";

        private static readonly Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
            [TextureEncoding.Format_Raw] = RawEncoding.Description,
            [TextureEncoding.Format_Albedo] = AlbedoEncoding.Description,
            [TextureEncoding.Format_Diffuse] = DiffuseEncoding.Description,
            [TextureEncoding.Format_Specular] = SpecularEncoding.Description,
            [TextureEncoding.Format_Legacy] = LegacyEncoding.Description,
            [TextureEncoding.Format_Lab11] = Lab11Encoding.Description,
            [TextureEncoding.Format_Lab12] = Lab12Encoding.Description,
            [TextureEncoding.Format_Lab13] = Lab13Encoding.Description,
        };


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return NoneDescription;

            if (value is string stringValue) {
                if (string.IsNullOrWhiteSpace(stringValue)) return NoneDescription;
                if (map.TryGetValue(stringValue, out var text)) return text;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
