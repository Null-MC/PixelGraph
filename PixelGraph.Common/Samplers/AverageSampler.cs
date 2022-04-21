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
            var pyMin = (int)fy;

            return new AverageRowSampler<TPixel> {
                Rows = Enumerable.Range(pyMin, stepY).Select(i => {
                    NormalizeTexCoordY(ref i);
                    return Image.DangerousGetPixelRowMemory(i);
                }).ToArray(),
                RangeX = RangeX,
                RangeY = RangeY,
                WrapX = WrapX,
                WrapY = WrapY,
                Bounds = Bounds,
                YMin = pyMin,
                YMax = pyMin + stepY,
            };
        }

        public override void Sample(in double x, in double y, ref Rgba32 pixel)
        {
            GetTexCoord(in x, in y, out var fx, out var fy);
            
            var minRangeX = MathF.Max(RangeX, 1f);
            var minRangeY = MathF.Max(RangeY, 1f);
            
            var stepX = (int)MathF.Ceiling(minRangeX);
            var stepY = (int)MathF.Ceiling(minRangeY);
            var weight = stepX * stepY;

            var pxMin = (int)fx; //(fx - 0.5f);
            var pyMin = (int)fy; //(fy - 0.5f);
            var pxMax = pxMin + stepX;
            var pyMax = pyMin + stepY;

            var color = new Vector4();
            for (var py = pyMin; py < pyMax; py++) {
                var _py = py;

                NormalizeTexCoordY(ref _py);

                var row = Image.DangerousGetPixelRowMemory(_py).Span;

                for (var px = pxMin; px < pxMax; px++) {
                    var _px = px;

                    NormalizeTexCoordX(ref _px);

                    color += row[_px].ToScaledVector4();
                }
            }

            pixel.FromScaledVector4(color / weight);
        }
    }

    internal struct AverageRowSampler<TPixel> : IRowSampler<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        public Memory<TPixel>[] Rows;
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

            var minRangeX = MathF.Max(RangeX, 1f);
            var minRangeY = MathF.Max(RangeY, 1f);
            
            var stepX = (int)MathF.Ceiling(minRangeX);
            var stepY = (int)MathF.Ceiling(minRangeY);
            var weight = stepX * stepY;

            var pxMin = (int)fx; //(fx - 0.5f);
            var pyMin = (int)fy; //(fy - 0.5f);
            //var pxMax = pxMin + stepX;
            var pyMax = pyMin + stepY;

            if (pyMin != YMin) throw new ApplicationException($"Sample row {pyMin} does not match RowSampler row {YMin}!");
            if (pyMax != YMax) throw new ApplicationException($"Sample row {pyMax} does not match RowSampler row {YMax}!");

            var color = new Vector4();
            for (var iy = 0; iy < stepY; iy++) {
                var py = pyMin + iy;
                var _py = py;

                if (WrapY) TexCoordHelper.WrapCoordY(ref _py, in Bounds);
                else TexCoordHelper.ClampCoordY(ref _py, in Bounds);

                //var row = Image.DangerousGetPixelRowMemory(_py).Span;
                var row = Rows[_py].Span;

                for (var ix = 0; ix < stepX; ix++) {
                    var px = pxMin + ix;
                    var _px = px;

                    if (WrapX) TexCoordHelper.WrapCoordX(ref _px, in Bounds);
                    else TexCoordHelper.ClampCoordX(ref _px, in Bounds);

                    color += row[_px].ToScaledVector4();
                }
            }

            pixel = color / weight;
        }
    }
}
