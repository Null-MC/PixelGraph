using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System;
using System.Collections.Generic;

namespace PixelGraph.Common.Textures
{
    public static class Samplers
    {
        public const string Nearest = "nearest";
        public const string Point = "point";
        public const string Box = "box";
        public const string Bilinear = "bilinear";
        public const string Triangle = "triangle";
        public const string Cubic = "cubic";
        public const string CatmullRom = "catmull-rom";
        public const string Bicubic = "bicubic";
        public const string Hermite = "hermite";
        public const string Spline = "spline";
        public const string Welch = "welch";
        public const string Lanczos2 = "lanczos-2";
        public const string Lanczos3 = "lanczos-3";
        public const string Lanczos5 = "lanczos-5";
        public const string Lanczos8 = "lanczos-8";

        private static readonly Dictionary<string, IResampler> map = new Dictionary<string, IResampler>(StringComparer.InvariantCultureIgnoreCase) {
            [Nearest] = KnownResamplers.NearestNeighbor,
            [Point] = KnownResamplers.NearestNeighbor,
            [Box] = KnownResamplers.Box,
            [Bilinear] = KnownResamplers.Triangle,
            [Triangle] = KnownResamplers.Triangle,
            [Cubic] = KnownResamplers.CatmullRom,
            [CatmullRom] = KnownResamplers.CatmullRom,
            [Bicubic] = KnownResamplers.Bicubic,
            [Hermite] = KnownResamplers.Hermite,
            [Spline] = KnownResamplers.Spline,
            [Welch] = KnownResamplers.Welch,
            [Lanczos2] = KnownResamplers.Lanczos2,
            [Lanczos3] = KnownResamplers.Lanczos3,
            [Lanczos5] = KnownResamplers.Lanczos5,
            [Lanczos8] = KnownResamplers.Lanczos8,
        };


        public static bool TryParse(string name, out IResampler sampler)
        {
            return map.TryGetValue(name, out sampler);
        }
    }
}
