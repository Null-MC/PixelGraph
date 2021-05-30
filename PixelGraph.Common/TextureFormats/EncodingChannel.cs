using System;
using System.Collections.Generic;

namespace PixelGraph.Common.TextureFormats
{
    public class EncodingChannel
    {
        public const string None = "none";
        //public const string White = "255";

        public const string Alpha = "alpha";
        public const string DiffuseRed = "diffuse-red";
        public const string DiffuseGreen = "diffuse-green";
        public const string DiffuseBlue = "diffuse-blue";
        public const string AlbedoRed = "albedo-red";
        public const string AlbedoGreen = "albedo-green";
        public const string AlbedoBlue = "albedo-blue";
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

        public static decimal GetDefaultValue(string encodingChannel)
        {
            return defaultValueMap.TryGetValue(encodingChannel, out var value) ? value : 0;
        }

        private static readonly Dictionary<string, byte> defaultValueMap = new(StringComparer.OrdinalIgnoreCase) {
            [Alpha] = 255,
            [AlbedoRed] = 0,
            [AlbedoGreen] = 0,
            [AlbedoBlue] = 0,
            [DiffuseRed] = 0,
            [DiffuseGreen] = 0,
            [DiffuseBlue] = 0,
            [Height] = 0,
            [NormalX] = 0,
            [NormalY] = 0,
            [NormalZ] = 0,
            [Occlusion] = 0,
            [Specular] = 0,
            [Smooth] = 0,
            [Rough] = 0,
            [F0] = 0,
            [Metal] = 0,
            [Porosity] = 0,
            [SubSurfaceScattering] = 0,
            [Emissive] = 0,
        };
    }
}
