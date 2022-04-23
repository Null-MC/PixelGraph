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
    internal class BicubicSampler<TPixel> : SamplerBase<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        public override IRowSampler<TPixel> ForRow(in double y)
        {
            GetTexCoordY(in y, out var fy);
            var pyMin = (int)MathF.Floor(fy - 1.5f);

            return new BicubicRowSampler<TPixel> {
                Rows = Enumerable.Range(pyMin, 4).Select(i => {
                    NormalizeTexCoordY(ref i);
                    return Image.DangerousGetPixelRowMemory(i)
                        .Slice(Bounds.X, Bounds.Width);
                }).ToArray(),
                //RangeX = RangeX,
                //RangeY = RangeY,
                WrapX = WrapX,
                Bounds = Bounds,
                YMin = pyMin,
            };
        }

        public override void SampleScaled(in double x, in double y, in ColorChannel color, out float pixelValue)
        {
            GetTexCoord(in x, in y, out var fx, out var fy);

            fx -= 1.5f;
            fy -= 1.5f;

            var pxMin = (int)MathF.Floor(fx);
            var pyMin = (int)MathF.Floor(fy);

            var px = fx - pxMin;
            var py = fy - pyMin;

            var k = new Vector4[4][];

            for (var ky = 0; ky < 4; ky++) {
                k[ky] = new Vector4[4];
                var ly = pyMin + ky;

                NormalizeTexCoordY(ref ly);

                var row = Image.DangerousGetPixelRowMemory(ly).Span;

                for (var kx = 0; kx < 4; kx++) {
                    var lx = pxMin + kx;

                    NormalizeTexCoordX(ref lx);

                    k[ky][kx] = row[lx].ToScaledVector4();
                }
            }

            var col = new Vector4[4];
            CubicHermite(in k[0], in px, out col[0]);
            CubicHermite(in k[1], in px, out col[1]);
            CubicHermite(in k[2], in px, out col[2]);
            CubicHermite(in k[3], in px, out col[3]);
            CubicHermite(in col, in py, out var vector);

            vector.GetChannelValue(color, out pixelValue);
        }

        private static void CubicHermite(in Vector4[] row, in float t, out Vector4 result)
        {
            result.X = CubicHermite(in row[0].X, in row[1].X, in row[2].X, in row[3].X, in t);
            result.Y = CubicHermite(in row[0].Y, in row[1].Y, in row[2].Y, in row[3].Y, in t);
            result.Z = CubicHermite(in row[0].Z, in row[1].Z, in row[2].Z, in row[3].Z, in t);
            result.W = CubicHermite(in row[0].W, in row[1].W, in row[2].W, in row[3].W, in t);
        }

        private static float CubicHermite(in float A, in float B, in float C, in float D, in float t)
        {
            var a = -A / 2f + 3f*B / 2f - 3f*C / 2f + D / 2f;
            var b = A - 5f*B / 2f + 2f*C - D / 2f;
            var c = -A / 2f + C / 2f;
 
            return a*t*t*t + b*t*t + c*t + B;
        }
    }

    internal struct BicubicRowSampler<TPixel> : IRowSampler<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        public Memory<TPixel>[] Rows;
        public Rectangle Bounds;
        public int YMin;

        //public float RangeX {get; set;}
        //public float RangeY {get; set;}
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
            var fx = (float)(Bounds.Left + x * Bounds.Width) - 1.5f;
            var fy = (float)(Bounds.Top + y * Bounds.Height) - 1.5f;

            var pxMin = (int)MathF.Floor(fx);
            var pyMin = (int)MathF.Floor(fy);

#if DEBUG
            if (pyMin != YMin) throw new ApplicationException($"Sample row {pyMin} does not match RowSampler row {YMin}!");
#endif

            var px = fx - pxMin;
            var py = fy - pyMin;

            var k = new Vector4[4][];
            for (var ky = 0; ky < 4; ky++) {
                k[ky] = new Vector4[4];

                var row = Rows[ky].Span;

                for (var kx = 0; kx < 4; kx++) {
                    var lx = pxMin + kx;

                    if (WrapX) TexCoordHelper.WrapCoordX(ref lx, in Bounds);
                    else TexCoordHelper.ClampCoordX(ref lx, in Bounds);

                    k[ky][kx] = row[lx - Bounds.X].ToScaledVector4();
                }
            }

            var col = new Vector4[4];
            CubicHermite(in k[0], in px, out col[0]);
            CubicHermite(in k[1], in px, out col[1]);
            CubicHermite(in k[2], in px, out col[2]);
            CubicHermite(in k[3], in px, out col[3]);
            CubicHermite(in col, in py, out pixel);
        }

        private static void CubicHermite(in Vector4[] row, in float t, out Vector4 result)
        {
            result.X = CubicHermite(in row[0].X, in row[1].X, in row[2].X, in row[3].X, in t);
            result.Y = CubicHermite(in row[0].Y, in row[1].Y, in row[2].Y, in row[3].Y, in t);
            result.Z = CubicHermite(in row[0].Z, in row[1].Z, in row[2].Z, in row[3].Z, in t);
            result.W = CubicHermite(in row[0].W, in row[1].W, in row[2].W, in row[3].W, in t);
        }

        private static float CubicHermite(in float A, in float B, in float C, in float D, in float t)
        {
            var a = -A / 2f + 3f*B / 2f - 3f*C / 2f + D / 2f;
            var b = A - 5f*B / 2f + 2f*C - D / 2f;
            var c = -A / 2f + C / 2f;
 
            return a*t*t*t + b*t*t + c*t + B;
        }
    }
}
