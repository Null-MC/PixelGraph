using PixelGraph.Common.Extensions;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;

namespace PixelGraph.Common.Samplers;

internal class BilinearSampler<TPixel> : SamplerBase<TPixel>
    where TPixel : unmanaged, IPixel<TPixel>
{
    public override IRowSampler ForRow(in double y)
    {
        ArgumentNullException.ThrowIfNull(Image);

        GetTexcoordY(in y, out var fy);

        var minRangeY = (int)MathF.Ceiling(RangeY);
        var pyMin = (int)MathF.Floor(fy - 0.5f*minRangeY);
        var pyMax = pyMin + minRangeY;

        var sampler = new BilinearRowSampler<TPixel> {
            RangeX = RangeX,
            RangeY = RangeY,
            WrapX = WrapX,
            Bounds = Bounds,
            YMin = pyMin,
            YMax = pyMax,
        };

        NormalizeTexcoordY(ref pyMin);
        NormalizeTexcoordY(ref pyMax);

        sampler.RowMin = Image
            .DangerousGetPixelRowMemory(pyMin)
            .Slice(Bounds.X, Bounds.Width);

        sampler.RowMax = Image
            .DangerousGetPixelRowMemory(pyMax)
            .Slice(Bounds.X, Bounds.Width);

        return sampler;
    }

    public override void SampleScaled(in double x, in double y, in ColorChannel color, out float pixelValue)
    {
        ArgumentNullException.ThrowIfNull(Image);

        GetTexCoord(in x, in y, out var fx, out var fy);

        fx -= 0.5f;

        var pxMin = (int)MathF.Floor(fx);
        var pxMax = pxMin + (int)MathF.Ceiling(RangeX);
        var pyMin = (int)MathF.Floor(fy);
        var pyMax = pyMin + (int)MathF.Ceiling(RangeY);

        var px = fx - pxMin;
        var py = fy - pyMin;

        NormalizeTexcoord(ref pxMin, ref pyMin);
        NormalizeTexcoord(ref pxMax, ref pyMax);

        var rowMin = Image
            .DangerousGetPixelRowMemory(pyMin)
            .Slice(Bounds.X, Bounds.Width).Span;
        var rowMax = Image
            .DangerousGetPixelRowMemory(pyMax)
            .Slice(Bounds.X, Bounds.Width).Span;

        var pixelMatrix = new Vector4[4];
        pixelMatrix[0] = rowMin[pxMin].ToScaledVector4();
        pixelMatrix[1] = rowMin[pxMax].ToScaledVector4();
        pixelMatrix[2] = rowMax[pxMin].ToScaledVector4();
        pixelMatrix[3] = rowMax[pxMax].ToScaledVector4();

        MathEx.Lerp(in pixelMatrix[0], in pixelMatrix[1], in px, out var zMin);
        MathEx.Lerp(in pixelMatrix[2], in pixelMatrix[3], in px, out var zMax);
        MathEx.Lerp(in zMin, in zMax, in py, out var vector);

        vector.GetChannelValue(color, out pixelValue);
    }
}

internal struct BilinearRowSampler<TPixel> : IRowSampler
    where TPixel : unmanaged, IPixel<TPixel>
{
    public Memory<TPixel> RowMin;
    public Memory<TPixel> RowMax;
    public Rectangle Bounds;
    public int YMin, YMax;

    public float RangeX {get; set;}
    public float RangeY {get; set;}
    public bool WrapX {get; set;}


    public void Sample(in double x, in double y, ref Rgba32 pixel)
    {
        SampleInternal(in x, in y, out var result);
        pixel.FromScaledVector4(result);
    }

    public void SampleScaled(in double x, in double y, out Vector4 pixel)
    {
        SampleInternal(in x, in y, out pixel);
    }

    public void Sample(in double x, in double y, in ColorChannel color, out byte pixelValue)
    {
        SampleInternal(in x, in y, out var result);
        result.GetChannelByteValue(in color, out pixelValue);
    }

    public void SampleScaled(in double x, in double y, in ColorChannel color, out float pixelValue)
    {
        SampleInternal(in x, in y, out var result);
        result.GetChannelValue(in color, out pixelValue);
    }

    private void SampleInternal(in double x, in double y, out Vector4 pixel)
    {
        var minRangeX = (int)MathF.Ceiling(RangeX);
        var minRangeY = (int)MathF.Ceiling(RangeY);

        var fx = (float)(Bounds.Left + x * Bounds.Width) - 0.5f*minRangeX;
        var fy = (float)(Bounds.Top + y * Bounds.Height) - 0.5f*minRangeY;

        var pxMin = (int)MathF.Floor(fx);
        var pxMax = pxMin + minRangeX;
        var pyMin = (int)MathF.Floor(fy);
        var pyMax = pyMin + minRangeY;

        var px = fx - pxMin;
        var py = fy - pyMin;

#if DEBUG
        if (pyMin != YMin) throw new ApplicationException($"Sample row {pyMin} does not match RowSampler row {YMin}!");
        if (pyMax != YMax) throw new ApplicationException($"Sample row {pyMax} does not match RowSampler row {YMax}!");
#endif

        if (WrapX) {
            TexCoordHelper.WrapCoordX(ref pxMin, in Bounds);
            TexCoordHelper.WrapCoordX(ref pxMax, in Bounds);
        }
        else {
            TexCoordHelper.ClampCoordX(ref pxMin, in Bounds);
            TexCoordHelper.ClampCoordX(ref pxMax, in Bounds);
        }

        var spanMin = RowMin.Span;
        var spanMax = RowMax.Span;
        var pixelMatrix = new Vector4[4];
        pixelMatrix[0] = spanMin[pxMin - Bounds.X].ToScaledVector4();
        pixelMatrix[1] = spanMin[pxMax - Bounds.X].ToScaledVector4();
        pixelMatrix[2] = spanMax[pxMin - Bounds.X].ToScaledVector4();
        pixelMatrix[3] = spanMax[pxMax - Bounds.X].ToScaledVector4();

        MathEx.Lerp(in pixelMatrix[0], in pixelMatrix[1], in px, out var zMin);
        MathEx.Lerp(in pixelMatrix[2], in pixelMatrix[3], in px, out var zMax);
        MathEx.Lerp(in zMin, in zMax, in py, out pixel);
    }
}