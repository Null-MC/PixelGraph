using PixelGraph.Common.Extensions;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;

namespace PixelGraph.Common.Samplers
{
    internal class NearestSampler<TPixel> : SamplerBase<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        public override IRowSampler<TPixel> ForRow(in double y)
        {
            GetTexCoordY(in y, out var fy);
            var py = (int)fy;

            NormalizeTexCoordY(ref py);

            return new NearestRowSampler<TPixel> {
                Row = Image.DangerousGetPixelRowMemory(py).Slice(Bounds.Left, Bounds.Width),
                //RangeX = RangeX,
                //RangeY = RangeY,
                WrapX = WrapX,
                WrapY = WrapY,
                Bounds = Bounds,
                Y = py,
            };
        }

        public override void Sample(in double x, in double y, ref Rgba32 pixel)
        {
            GetTexCoord(in x, in y, out var fx, out var fy);

            var px = (int)fx;
            var py = (int)fy;

            NormalizeTexCoord(ref px, ref py);

            Image[px, py].ToRgba32(ref pixel);
        }
    }

    internal struct NearestRowSampler<TPixel> : IRowSampler<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        public Memory<TPixel> Row;
        public Rectangle Bounds;
        public int Y;

        //public float RangeX {get; set;}
        //public float RangeY {get; set;}
        public bool WrapX {get; set;}
        public bool WrapY {get; set;}


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

            var px = (int)fx;
            var py = (int)fy;

            if (WrapX) TexCoordHelper.WrapCoordX(ref px, in Bounds);
            else TexCoordHelper.ClampCoordX(ref px, in Bounds);

            if (WrapY) TexCoordHelper.WrapCoordY(ref py, in Bounds);
            else TexCoordHelper.ClampCoordY(ref py, in Bounds);

            if (py != Y) throw new ApplicationException($"Sample row {py} does not match RowSampler row {Y}!");

            return Row.Span[px];
        }
    }
}
