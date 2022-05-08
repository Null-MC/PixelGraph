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
    internal class WeightedAverageSampler<TPixel> : SamplerBase<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        public float NearestWeight = 0.5f;


        public override IRowSampler<TPixel> ForRow(in double y)
        {
            GetTexCoordY(in y, out var fy);

            var minRangeY = MathF.Max(RangeY, 1f);
            var stepY = (int)MathF.Ceiling(minRangeY);
            var pyMin = (int)MathF.Floor(fy - 0.5f*stepY);

            var rows = Enumerable.Range(pyMin, stepY).Select(i => {
                    NormalizeTexCoordY(ref i);
                    return Image.DangerousGetPixelRowMemory(i)
                        .Slice(Bounds.X, Bounds.Width);
                }).ToArray();

            return new WeightedAverageRowSampler<TPixel>(in rows, in Bounds) {
                NearestWeight = NearestWeight,
                RangeX = RangeX,
                RangeY = RangeY,
                WrapX = WrapX,
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

            var avgWeight = 0f;
            if (weight > 1) avgWeight = 1f / (weight - 1);
            avgWeight *= 1f - NearestWeight;

            var nearWeight = weight > 1 ? NearestWeight : 1f;

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

                    if (iy == 0 && ix == 0) {
                        pixelValue += pValue * nearWeight;
                    }
                    else {
                        pixelValue += pValue * avgWeight;
                    }
                }
            }
        }
    }

    internal struct WeightedAverageRowSampler<TPixel> : IRowSampler<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly Memory<TPixel>[] rows;
        private readonly Rectangle bounds;

        public float NearestWeight;
        public float RangeX;
        public float RangeY;
        public bool WrapX;
        public int YMin;


        public WeightedAverageRowSampler(in Memory<TPixel>[] rows, in Rectangle bounds)
        {
            this.rows = rows;
            this.bounds = bounds;

            NearestWeight = 0f;
            RangeX = 1;
            RangeY = 1;
            WrapX = true;
            YMin = 0;
        }

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
            var fx = (float)(bounds.Left + x * bounds.Width);
            var fy = (float)(bounds.Top + y * bounds.Height);

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

            var avgWeight = 0f;
            if (weight > 1) avgWeight = 1f / (weight - 1);
            avgWeight *= 1f - NearestWeight;

            var nearWeight = weight > 1 ? NearestWeight : 1f;

            var cx = (int)Math.Floor(stepX * 0.5f + float.Epsilon);
            var cy = (int)Math.Floor(stepY * 0.5f + float.Epsilon);

            pixel = Vector4.Zero;
            for (var iy = 0; iy < stepY; iy++) {
                var row = rows[iy].Span;

                for (var ix = 0; ix < stepX; ix++) {
                    var px = pxMin + ix;

                    if (WrapX) TexCoordHelper.WrapCoordX(ref px, in bounds);
                    else TexCoordHelper.ClampCoordX(ref px, in bounds);

                    var pixelValue = row[px - bounds.X].ToScaledVector4();

                    if (iy == cy && ix == cx) {
                        pixel += pixelValue * nearWeight;
                    }
                    else {
                        pixel += pixelValue * avgWeight;
                    }
                }
            }
        }
    }
}
