using PixelGraph.Common.TextureFormats;
using PixelGraph.Common.TextureFormats.Bedrock;
using PixelGraph.Common.TextureFormats.Java;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace PixelGraph.UI.Converters
{
    internal class FormatDescriptionConverter : IValueConverter
    {
        public const string NoneDescription = "No channel mappings. Do not use this without manually specifying encoding channels.";

        private static readonly Dictionary<string, string> map = new(StringComparer.InvariantCultureIgnoreCase) {
            [TextureFormat.Format_Raw] = RawFormat.Description,
            [TextureFormat.Format_Color] = ColorFormat.Description,
            [TextureFormat.Format_Specular] = SpecularFormat.Description,
            [TextureFormat.Format_OldPbr] = OldPbrFormat.Description,
            [TextureFormat.Format_Lab11] = LabPbr11Format.Description,
            [TextureFormat.Format_Lab12] = LabPbr12Format.Description,
            [TextureFormat.Format_Lab13] = LabPbr13Format.Description,
            [TextureFormat.Format_Rtx] = RtxFormat.Description,
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
