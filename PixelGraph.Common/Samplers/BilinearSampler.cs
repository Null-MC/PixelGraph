using PixelGraph.Common.Extensions;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;

namespace PixelGraph.Common.Samplers
{
    internal class BilinearSampler<TPixel> : SamplerBase<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        public override IRowSampler<TPixel> ForRow(in double y)
        {
            GetTexCoordY(in y, out var fy);

            var pyMin = (int)fy;
            var pyMax = pyMin + 1;

            NormalizeTexCoordY(ref pyMin);
            NormalizeTexCoordY(ref pyMax);

            return new BilinearRowSampler<TPixel> {
                RowMin = Image.DangerousGetPixelRowMemory(pyMin),
                RowMax = Image.DangerousGetPixelRowMemory(pyMax),
                RangeX = RangeX,
                RangeY = RangeY,
                WrapX = WrapX,
                WrapY = WrapY,
                Bounds = Bounds,
                YMin = pyMin,
                YMax = pyMax,
            };
        }

        public override void Sample(in double x, in double y, ref Rgba32 pixel)
        {
            SampleScaled(in x, in y, out var vector);
            pixel.FromScaledVector4(vector);
        }

        public override void SampleScaled(in double x, in double y, out Vector4 vector)
        {
            GetTexCoord(in x, in y, out var fx, out var fy);

            var pxMin = (int)fx; //(fx + 0.5f);
            var pxMax = pxMin + 1;
            var pyMin = (int)fy; //(fy + 0.5f);
            var pyMax = pyMin + 1;

            var px = fx - pxMin;// - 0.5f;
            var py = fy - pyMin;// - 0.5f;

            NormalizeTexCoord(ref pxMin, ref pyMin);
            NormalizeTexCoord(ref pxMax, ref pyMax);

            var rowMin = Image.DangerousGetPixelRowMemory(pyMin).Span;
            var rowMax = Image.DangerousGetPixelRowMemory(pyMax).Span;

            var pixelMatrix = new Vector4[4];
            pixelMatrix[0] = rowMin[pxMin].ToScaledVector4();
            pixelMatrix[1] = rowMin[pxMax].ToScaledVector4();
            pixelMatrix[2] = rowMax[pxMin].ToScaledVector4();
            pixelMatrix[3] = rowMax[pxMax].ToScaledVector4();

            MathEx.Lerp(in pixelMatrix[0], in pixelMatrix[1], in px, out var zMin);
            MathEx.Lerp(in pixelMatrix[2], in pixelMatrix[3], in px, out var zMax);
            MathEx.Lerp(in zMin, in zMax, in py, out vector);
        }

        public override void Sample(in double fx, in double fy, in ColorChannel color, out byte pixelValue)
        {
            var pixel = new Rgba32();
            Sample(in fx, in fy, ref pixel);
            pixel.GetChannelValue(color, out pixelValue);
        }

        public override void SampleScaled(in double fx, in double fy, in ColorChannel color, out float pixelValue)
        {
            SampleScaled(in fx, in fy, out var vector);
            vector.GetChannelValue(color, out pixelValue);
        }
    }

    internal struct BilinearRowSampler<TPixel> : IRowSampler<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        public Memory<TPixel> RowMin;
        public Memory<TPixel> RowMax;
        public Rectangle Bounds;
        public int YMin, YMax;

        public float RangeX {get; set;}
        public float RangeY {get; set;}
        public bool WrapX {get; set;}
        public bool WrapY {get; set;}


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
            var fx = (float)(Bounds.Left + x * Bounds.Width);
            var fy = (float)(Bounds.Top + y * Bounds.Height);

            var pxMin = (int)fx; //(fx + 0.5f);
            var pxMax = pxMin + 1;
            var pyMin = (int)fy; //(fy + 0.5f);
            var pyMax = pyMin + 1;

            var px = fx - pxMin;// - 0.5f;
            var py = fy - pyMin;// - 0.5f;

            if (WrapX) {
                TexCoordHelper.WrapCoordX(ref pxMin, in Bounds);
                TexCoordHelper.WrapCoordX(ref pxMax, in Bounds);
            }
            else {
                TexCoordHelper.ClampCoordX(ref pxMin, in Bounds);
                TexCoordHelper.ClampCoordX(ref pxMax, in Bounds);
            }

            if (WrapY) {
                TexCoordHelper.WrapCoordY(ref pyMin, in Bounds);
                TexCoordHelper.WrapCoordY(ref pyMax, in Bounds);
            }
            else {
                TexCoordHelper.ClampCoordY(ref pyMin, in Bounds);
                TexCoordHelper.ClampCoordY(ref pyMax, in Bounds);
            }

            if (pyMin != YMin) throw new ApplicationException($"Sample row {pyMin} does not match RowSampler row {YMin}!");
            if (pyMax != YMax) throw new ApplicationException($"Sample row {pyMax} does not match RowSampler row {YMax}!");

            var spanMin = RowMin.Span;
            var spanMax = RowMax.Span;
            var pixelMatrix = new Vector4[4];
            pixelMatrix[0] = spanMin[pxMin].ToScaledVector4();
            pixelMatrix[1] = spanMin[pxMax].ToScaledVector4();
            pixelMatrix[2] = spanMax[pxMin].ToScaledVector4();
            pixelMatrix[3] = spanMax[pxMax].ToScaledVector4();

            MathEx.Lerp(in pixelMatrix[0], in pixelMatrix[1], in px, out var zMin);
            MathEx.Lerp(in pixelMatrix[2], in pixelMatrix[3], in px, out var zMax);
            MathEx.Lerp(in zMin, in zMax, in py, out pixel);
        }
    }
}
