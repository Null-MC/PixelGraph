using System;

namespace PixelGraph.Common.Encoding
{
    public class EncodingChannel
    {
        public const string None = "none";
        public const string Black = "0";
        public const string White = "255";

        public const string Red = "red";
        public const string Green = "green";
        public const string Blue = "blue";
        public const string Alpha = "alpha";
        public const string Height = "height";
        public const string NormalX = "normal-x";
        public const string NormalY = "normal-y";
        public const string NormalZ = "normal-z";
        public const string Occlusion = "occlusion";
        public const string Specular = "specular";
        public const string Smooth = "smooth";
        public const string Rough = "rough";
        public const string PerceptualSmooth = "smooth2";
        public const string Metal = "metal";
        public const string Emissive = "emissive";
        public const string EmissiveClipped = "emissive-clip";
        public const string EmissiveInverse = "emissive-inv";
        public const string Porosity = "porosity";
        public const string Porosity_SSS = "porosity-sss";
        public const string SubSurfaceScattering = "sss";


        public static bool Is(string channelActual, string channelExpected)
        {
            return string.Equals(channelActual, channelExpected, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsEmpty(string channel)
        {
            return string.IsNullOrWhiteSpace(channel) || string.Equals(channel, None, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
