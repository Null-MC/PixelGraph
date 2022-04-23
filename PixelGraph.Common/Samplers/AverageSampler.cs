using PixelGraph.Common.Extensions;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Linq;
using System.Numerics;

namespace PixelGraph.Common.Samplers
{
    internal class AverageSampler<TPixel> : SamplerBase<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        public override IRowSampler<TPixel> ForRow(in double y)
        {
            GetTexCoordY(in y, out var fy);

            var minRangeY = MathF.Max(RangeY, 1f);
            var stepY = (int)MathF.Ceiling(minRangeY);
            var pyMin = (int)MathF.Floor(fy - 0.5f*stepY);

            return new AverageRowSampler<TPixel> {
                Rows = Enumerable.Range(pyMin, stepY).Select(i => {
                    NormalizeTexCoordY(ref i);
                    return Image.DangerousGetPixelRowMemory(i)
                        .Slice(Bounds.X, Bounds.Width);
                }).ToArray(),
                RangeX = RangeX,
                RangeY = RangeY,
                WrapX = WrapX,
                Bounds = Bounds,
                YMin = pyMin,
            };
        }

        public override void SampleScaled(in double x, in double y, in ColorChannel color, out float pixelValue)
        {
            GetTexCoord(in x, in y, out var fx, out var fy);
            
            var minRangeX = MathF.Max(RangeX, 1f);
            var minRangeY = MathF.Max(RangeY, 1f);
            
            var stepX = (int)MathF.Ceiling(minRangeX);
            var stepY = (int)MathF.Ceiling(minRangeY);
            var weight = stepX * stepY;

            var pxMin = (int)MathF.Floor(fx - 0.5f*stepX);
            var pyMin = (int)MathF.Floor(fy - 0.5f*stepY);

            pixelValue = 0f;
            for (var iy = 0; iy < stepX; iy++) {
                var py = pyMin + iy;
                NormalizeTexCoordY(ref py);

                var row = Image
                    .DangerousGetPixelRowMemory(py)
                    .Slice(Bounds.X, Bounds.Width).Span;

                for (var ix = 0; ix < stepX; ix++) {
                    var px = pxMin + ix;
                    NormalizeTexCoordX(ref px);

                    var p = row[px - Bounds.X].ToScaledVector4();
                    p.GetChannelValue(color, out var pValue);
                    pixelValue += pValue;
                }
            }

            pixelValue /= weight;
        }
    }

    internal struct AverageRowSampler<TPixel> : IRowSampler<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        public Memory<TPixel>[] Rows;
        public Rectangle Bounds;
        public int YMin;

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
            var fx = (float)(Bounds.Left + x * Bounds.Width);
            var fy = (float)(Bounds.Top + y * Bounds.Height);

            var minRangeX = MathF.Max(RangeX, 1f);
            var minRangeY = MathF.Max(RangeY, 1f);
            
            var stepX = (int)MathF.Ceiling(minRangeX);
            var stepY = (int)MathF.Ceiling(minRangeY);
            var weight = stepX * stepY;

            var pxMin = (int)MathF.Floor(fx - 0.5f*stepX);
            var pyMin = (int)MathF.Floor(fy - 0.5f*stepY);

#if DEBUG
            if (pyMin != YMin) throw new ApplicationException($"Sample row {pyMin} does not match RowSampler row {YMin}!");
#endif

            var color = new Vector4();
            for (var iy = 0; iy < stepY; iy++) {
                var row = Rows[iy].Span;

                for (var ix = 0; ix < stepX; ix++) {
                    var px = pxMin + ix;

                    if (WrapX) TexCoordHelper.WrapCoordX(ref px, in Bounds);
                    else TexCoordHelper.ClampCoordX(ref px, in Bounds);

                    color += row[px - Bounds.X].ToScaledVector4();
                }
            }

            pixel = color / weight;
        }
    }
}
