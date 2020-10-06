using System;

namespace McPbrPipeline.Internal.Encoding
{
    internal class EncodingChannel
    {
        public const string None = "none";
        public const string Black = "0";
        public const string White = "255";

        public const string AlbedoR = "albedo-r";
        public const string AlbedoG = "albedo-g";
        public const string AlbedoB = "albedo-b";
        public const string AlbedoA = "albedo-a";
        public const string Height = "height";
        public const string NormalX = "normal-x";
        public const string NormalY = "normal-y";
        public const string NormalZ = "normal-z";
        public const string AmbientOcclusion = "ao";
        public const string Smooth = "smooth";
        public const string Rough = "rough";
        public const string PerceptualSmooth = "smooth2";
        public const string Reflect = "reflect";
        public const string Emissive = "emissive";
        public const string Porosity = "porosity";
        public const string Porosity_SSS = "porosity-sss";
        public const string SubSurfaceScattering = "sss";


        public static bool IsEmpty(string channel)
        {
            return string.IsNullOrWhiteSpace(channel) || string.Equals(channel, None, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
