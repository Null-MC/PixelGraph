using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace PixelGraph.Common.Samplers
{
    public static class Sampler
    {
        //public const string Point = "point";
        public const string Nearest = "nearest";
        public const string Bilinear = "bilinear";

        //public const string Box = "box";
        //public const string Triangle = "triangle";
        //public const string Cubic = "cubic";
        //public const string CatmullRom = "catmull-rom";
        //public const string Bicubic = "bicubic";
        //public const string Hermite = "hermite";
        //public const string Spline = "spline";
        //public const string Welch = "welch";
        //public const string Lanczos2 = "lanczos-2";
        //public const string Lanczos3 = "lanczos-3";
        //public const string Lanczos5 = "lanczos-5";
        //public const string Lanczos8 = "lanczos-8";


        public static ISampler Create(string name)
        {
            if (name == null) return null;
            return map.TryGetValue(name, out var samplerFunc) ? samplerFunc() : null;
        }

        private static readonly Dictionary<string, Func<ISampler>> map = new Dictionary<string, Func<ISampler>>(StringComparer.InvariantCultureIgnoreCase) {
            //[Point] = () => new PointSampler(),
            [Nearest] = () => new NearestSampler(),
            [Bilinear] = () => new BilinearSampler(),

            //[Box] = KnownResamplers.Box,
            //[Triangle] = KnownResamplers.Triangle,
            //[Cubic] = KnownResamplers.CatmullRom,
            //[CatmullRom] = KnownResamplers.CatmullRom,
            //[Bicubic] = KnownResamplers.Bicubic,
            //[Hermite] = KnownResamplers.Hermite,
            //[Spline] = KnownResamplers.Spline,
            //[Welch] = KnownResamplers.Welch,
            //[Lanczos2] = KnownResamplers.Lanczos2,
            //[Lanczos3] = KnownResamplers.Lanczos3,
            //[Lanczos5] = KnownResamplers.Lanczos5,
            //[Lanczos8] = KnownResamplers.Lanczos8,
        };
    }

    public interface ISampler
    {
        Image<Rgba32> Image {get; set;}
        bool Wrap {get; set;}

        void Sample(in float fx, in float fy, out Rgba32 pixel);
        void SampleScaled(in float fx, in float fy, out Vector4 pixel);

        void Sample(in float fx, in float fy, in ColorChannel color, out byte pixelValue);
        void SampleScaled(in float fx, in float fy, in ColorChannel color, out float pixelValue);
    }
}
