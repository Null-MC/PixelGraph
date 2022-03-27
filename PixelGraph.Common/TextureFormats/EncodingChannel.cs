using System;

namespace PixelGraph.Common.TextureFormats
{
    public class EncodingChannel
    {
        public const string None = "none";
        public const string Opacity = "opacity";
        public const string ColorRed = "color-red";
        public const string ColorGreen = "color-green";
        public const string ColorBlue = "color-blue";
        public const string Height = "height";
        public const string Bump = "bump";
        public const string Occlusion = "occlusion";
        public const string NormalX = "normal-x";
        public const string NormalY = "normal-y";
        public const string NormalZ = "normal-z";
        public const string Specular = "specular";
        public const string Smooth = "smooth";
        public const string Rough = "rough";
        public const string Metal = "metal";
        public const string HCM = "hcm";
        public const string F0 = "f0";
        public const string Porosity = "porosity";
        public const string SubSurfaceScattering = "sss";
        public const string Emissive = "emissive";


        public static bool Is(string channelActual, string channelExpected)
        {
            return string.Equals(channelActual, channelExpected, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsEmpty(string channel)
        {
            return string.IsNullOrWhiteSpace(channel) || string.Equals(channel, None, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsColor(string channel)
        {
            return Is(channel, ColorRed) || Is(channel, ColorGreen) || Is(channel, ColorBlue);
        }

        //public static byte? GetDefaultValue(string encodingChannel)
        //{
        //    return defaultValueMap.TryGetValue(encodingChannel, out var value) ? value : null;
        //}

        //private static readonly Dictionary<string, byte?> defaultValueMap = new(StringComparer.OrdinalIgnoreCase) {
        //    [Opacity] = 255,
        //    [Rough] = 255,
        //};
    }
}
