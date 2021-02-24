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
        public const string Nearest = "nearest";
        public const string Bilinear = "bilinear";
        public const string Average = "average";
    }

    public static class Sampler<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        public static ISampler<TPixel> Create(string name)
        {
            return TryCreate(name) ?? throw new ApplicationException($"Unknown sampler '{name}'!");
        }

        public static ISampler<TPixel> TryCreate(string name)
        {
            if (name == null) return null;
            return map.TryGetValue(name, out var samplerFunc) ? samplerFunc() : null;
        }

        private static readonly Dictionary<string, Func<ISampler<TPixel>>> map = new Dictionary<string, Func<ISampler<TPixel>>>(StringComparer.InvariantCultureIgnoreCase) {
            [Sampler.Nearest] = () => new NearestSampler<TPixel>(),
            [Sampler.Bilinear] = () => new BilinearSampler<TPixel>(),
            [Sampler.Average] = () => new AverageSampler<TPixel>(),
        };
    }

    public interface ISampler<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        Image<TPixel> Image {get; set;}
        float RangeX {get; set;}
        float RangeY {get; set;}
        bool WrapX {get; set;}
        bool WrapY {get; set;}

        void Sample(in float x, in float y, ref Rgba32 pixel);
        void SampleScaled(in float x, in float y, out Vector4 pixel);

        void Sample(in float x, in float y, in ColorChannel color, out byte pixelValue);
        void SampleScaled(in float x, in float y, in ColorChannel color, out float pixelValue);
    }
}
