using PixelGraph.Common.Extensions;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;

namespace PixelGraph.Common.Samplers
{
    internal abstract class SamplerBase<TPixel> : ISampler<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        protected const float HalfPixel = 0.5f + float.Epsilon;

        public Image<TPixel> Image {get; set;}
        public float RangeX {get; set;}
        public float RangeY {get; set;}
        public bool WrapX {get; set;}
        public bool WrapY {get; set;}
        public int? Frame {get; set;}
        public int FrameCount {get; set;}


        public abstract void Sample(in float x, in float y, ref Rgba32 pixel);

        public virtual void SampleScaled(in float x, in float y, out Vector4 vector)
        {
            var pixel = new Rgba32();
            Sample(in x, in y, ref pixel);
            vector = pixel.ToScaledVector4();
        }

        public virtual void Sample(in float x, in float y, in ColorChannel color, out byte pixelValue)
        {
            var pixel = new Rgba32();
            Sample(in x, in y, ref pixel);
            pixel.GetChannelValue(in color, out pixelValue);
        }

        public virtual void SampleScaled(in float x, in float y, in ColorChannel color, out float pixelValue)
        {
            Sample(in x, in y, in color, out var _value);
            pixelValue = _value / 255f;
        }

        protected void GetTexCoord(in float x, in float y, out float fx, out float fy)
        {
            if (Frame.HasValue) {
                var bounds = GetFrameBounds();
                fx = bounds.X + x * bounds.Width - HalfPixel;
                fy = bounds.Y + y * bounds.Height - HalfPixel;
            }
            else {
                fx = x * Image.Width - HalfPixel;
                fy = y * Image.Height - HalfPixel;
            }
        }

        protected Rectangle GetFrameBounds()
        {
            var frameHeight = Image.Height;
            if (FrameCount > 1) frameHeight /= FrameCount;

            var f = (Frame ?? 0) % FrameCount;
            return new Rectangle(0, f * frameHeight, Image.Width, frameHeight);
        }

        protected void WrapCoordX(ref int x)
        {
            while (x < 0) x += Image.Width;
            while (x >= Image.Width) x -= Image.Width;
        }

        protected void WrapCoordX(ref int x, ref Rectangle bounds)
        {
            while (x < bounds.Left) x += bounds.Width;
            while (x >= bounds.Right) x -= bounds.Width;
        }

        protected void WrapCoordY(ref int y)
        {
            while (y < 0) y += Image.Height;
            while (y >= Image.Height) y -= Image.Height;
        }

        protected void WrapCoordY(ref int y, ref Rectangle bounds)
        {
            while (y < bounds.Top) y += bounds.Height;
            while (y >= bounds.Bottom) y -= bounds.Height;
        }

        protected void ClampCoordX(ref int x)
        {
            if (x < 0) x = 0;
            if (x >= Image.Width) x = Image.Width - 1;
        }

        protected void ClampCoordX(ref int x, ref Rectangle bounds)
        {
            if (x < bounds.Left) x = bounds.Left;
            if (x >= bounds.Right) x = bounds.Right - 1;
        }

        protected void ClampCoordY(ref int y)
        {
            if (y < 0) y = 0;
            if (y >= Image.Height) y = Image.Height - 1;
        }

        protected void ClampCoordY(ref int y, ref Rectangle bounds)
        {
            if (y < bounds.Top) y = bounds.Top;
            if (y >= bounds.Bottom) y = bounds.Bottom - 1;
        }
    }
}
