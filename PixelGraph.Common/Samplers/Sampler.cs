using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace PixelGraph.Common.Samplers;

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

    private static readonly Dictionary<string, Func<ISampler<TPixel>>> map = new(StringComparer.InvariantCultureIgnoreCase) {
        [Samplers.Nearest] = () => new NearestSampler<TPixel>(),
        [Samplers.Bilinear] = () => new BilinearSampler<TPixel>(),
        [Samplers.Bicubic] = () => new BicubicSampler<TPixel>(),
        [Samplers.Average] = () => new AverageSampler<TPixel>(),
        [Samplers.WeightedAverage] = () => new WeightedAverageSampler<TPixel>(),
    };
}

public interface ISampler
{
    float RangeX {get; set;}
    float RangeY {get; set;}
    bool WrapX {get; set;}
    bool WrapY {get; set;}

    void SetBounds(in UVRegion region);
    IRowSampler ForRow(in double y);

    void SampleScaled(in double x, in double y, in ColorChannel color, out float pixelValue);
}

public interface ISampler<TPixel> : ISampler
    where TPixel : unmanaged, IPixel<TPixel>
{
    Image<TPixel> Image {get; set;}
}

public interface IRowSampler
{
    void Sample(in double x, in double y, ref Rgba32 pixel);
    void SampleScaled(in double x, in double y, out Vector4 pixel);

    void Sample(in double x, in double y, in ColorChannel color, out byte pixelValue);
    void SampleScaled(in double x, in double y, in ColorChannel color, out float pixelValue);
}