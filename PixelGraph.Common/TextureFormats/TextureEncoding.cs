using PixelGraph.Common.TextureFormats.Bedrock;
using PixelGraph.Common.TextureFormats.Java;
using System;
using System.Collections.Generic;

namespace PixelGraph.Common.TextureFormats
{
    public class TextureEncoding
    {
        public const string DefaultFormat = Format_Raw;

        public const string Format_Raw = "raw";
        public const string Format_Diffuse = "diffuse";
        public const string Format_Specular = "specular";
        public const string Format_OldPbr = "old-pbr";
        public const string Format_Lab11 = "lab-1.1";
        public const string Format_Lab12 = "lab-1.2";
        public const string Format_Lab13 = "lab-1.3";
        public const string Format_Rtx = "rtx";


        public static bool Is(string formatActual, string formatExpected) =>
            string.Equals(formatActual, formatExpected, StringComparison.InvariantCultureIgnoreCase);

        public static ITextureFormatFactory GetFactory(string format)
        {
            if (format == null) return null;
            return formatMap.TryGetValue(format, out var textureFormat) ? textureFormat : null;
        }

        private static readonly Dictionary<string, ITextureFormatFactory> formatMap =
            new(StringComparer.InvariantCultureIgnoreCase) {
                [Format_Raw] = new RawFormat(),
                [Format_Diffuse] = new DiffuseFormat(),
                [Format_Specular] = new SpecularFormat(),
                [Format_OldPbr] = new OldPbrFormat(),
                [Format_Lab11] = new LabPbr11Format(),
                [Format_Lab12] = new LabPbr12Format(),
                [Format_Lab13] = new LabPbr13Format(),
                [Format_Rtx] = new RtxFormat(),
            };
    }
}
