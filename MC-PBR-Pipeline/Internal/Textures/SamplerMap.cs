using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System;
using System.Collections.Generic;

namespace McPbrPipeline.Internal.Textures
{
    internal static class Samplers
    {
        private static readonly Dictionary<string, IResampler> map = new Dictionary<string, IResampler>(StringComparer.InvariantCultureIgnoreCase) {
            ["point"] = KnownResamplers.NearestNeighbor,
            ["box"] = KnownResamplers.Box,
            ["bilinear"] = KnownResamplers.Triangle,
            ["triangle"] = KnownResamplers.Triangle,
            ["cubic"] = KnownResamplers.CatmullRom,
            ["catmull-rom"] = KnownResamplers.CatmullRom,
            ["bicubic"] = KnownResamplers.Bicubic,
            ["hermite"] = KnownResamplers.Hermite,
            ["spline"] = KnownResamplers.Spline,
            ["welch"] = KnownResamplers.Welch,
            ["lanczos-2"] = KnownResamplers.Lanczos2,
            ["lanczos-3"] = KnownResamplers.Lanczos3,
            ["lanczos-5"] = KnownResamplers.Lanczos5,
            ["lanczos-8"] = KnownResamplers.Lanczos8,
        };

        public static bool TryParse(string name, out IResampler sampler)
        {
            return map.TryGetValue(name, out sampler);
        }
    }
}
