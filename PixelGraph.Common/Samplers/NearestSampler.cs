using PixelGraph.Common.Extensions;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;

namespace PixelGraph.Common.Samplers;

internal class NearestSampler<TPixel> : SamplerBase<TPixel>
    where TPixel : unmanaged, IPixel<TPixel>
{
    public override IRowSampler ForRow(in double y)
    {
        GetTexCoordY(in y, out var fy);
        var py = (int)MathF.Floor(fy);

        var sampler = new NearestRowSampler<TPixel> {
            WrapX = WrapX,
            Bounds = Bounds,
            Y = py,
        };

        NormalizeTexCoordY(ref py);

        sampler.Row = Image.DangerousGetPixelRowMemory(py)
            .Slice(Bounds.Left, Bounds.Width);

        return sampler;
    }

    public override void SampleScaled(in double x, in double y, in ColorChannel color, out float pixelValue)
    {
        GetTexCoord(in x, in y, out var fx, out var fy);

        var px = (int)MathF.Floor(fx);
        var py = (int)MathF.Floor(fy);

        NormalizeTexCoord(ref px, ref py);

        var pixel = Image[px, py].ToScaledVector4();
        pixel.GetChannelValue(color, out pixelValue);
    }
}

internal struct NearestRowSampler<TPixel> : IRowSampler
    where TPixel : unmanaged, IPixel<TPixel>
{
    public Memory<TPixel> Row;
    public Rectangle Bounds;
    public int Y;

    public bool WrapX {get; set;}


    public void Sample(in double x, in double y, ref Rgba32 pixel)
    {
        SampleInternal(in x, in y).ToRgba32(ref pixel);
    }

    public void SampleScaled(in double x, in double y, out Vector4 pixel)
    {
        pixel = SampleInternal(in x, in y).ToScaledVector4();
    }

    public void Sample(in double x, in double y, in ColorChannel color, out byte pixelValue)
    {
        var pixel = new Rgba32();
        SampleInternal(in x, in y).ToRgba32(ref pixel);
        pixel.GetChannelValue(in color, out pixelValue);
    }

    public void SampleScaled(in double x, in double y, in ColorChannel color, out float pixelValue)
    {
        var pixel = SampleInternal(in x, in y).ToScaledVector4();
        pixel.GetChannelValue(in color, out pixelValue);
    }

    private TPixel SampleInternal(in double x, in double y)
    {
        var fx = (float)(Bounds.Left + x * Bounds.Width);
        var fy = (float)(Bounds.Top + y * Bounds.Height);

        var px = (int)MathF.Floor(fx);
        var py = (int)MathF.Floor(fy);

#if DEBUG
        if (py != Y) throw new ApplicationException($"Sample row {py} does not match RowSampler row {Y}!");
#endif

        if (WrapX) TexCoordHelper.WrapCoordX(ref px, in Bounds);
        else TexCoordHelper.ClampCoordX(ref px, in Bounds);

        return Row.Span[px - Bounds.X];
    }
}