using System;
using System.Collections.Generic;

namespace PixelGraph.Common.Encoding
{
    public class TextureEncoding
    {
        public const string Format_Raw = "raw";
        public const string Format_Default = "default";
        public const string Format_Legacy = "legacy";
        public const string Format_Lab11 = "lab-1.1";
        public const string Format_Lab13 = "lab-1.3";


        public static ITextureEncodingFactory GetFactory(string format)
        {
            if (format == null) return null;
            return formatMap.TryGetValue(format, out var textureFormat) ? textureFormat : null;
        }

        private static readonly Dictionary<string, ITextureEncodingFactory> formatMap =
            new Dictionary<string, ITextureEncodingFactory>(StringComparer.InvariantCultureIgnoreCase) {
                [Format_Raw] = new RawEncoding(),
                [Format_Default] = new DefaultEncoding(),
                [Format_Legacy] = new LegacyEncoding(),
                [Format_Lab11] = new Lab11Encoding(),
                [Format_Lab13] = new Lab13Encoding(),
            };
    }
}
