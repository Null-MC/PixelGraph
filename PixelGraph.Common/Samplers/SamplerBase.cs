using PixelGraph.Common.Extensions;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;

namespace PixelGraph.Common.Samplers
{
    internal abstract class SamplerBase<TPixel> : ISampler<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        //protected const float HalfPixel = 0.5f + float.Epsilon;

        public Image<TPixel> Image {get; set;}
        public float RangeX {get; set;}
        public float RangeY {get; set;}
        public bool WrapX {get; set;}
        public bool WrapY {get; set;}
        public RectangleF Bounds {get; set;}


        public abstract void Sample(in double x, in double y, ref Rgba32 pixel);

        public virtual void SampleScaled(in double x, in double y, out Vector4 vector)
        {
            var pixel = new Rgba32();
            Sample(in x, in y, ref pixel);
            vector = pixel.ToScaledVector4();
        }

        public virtual void Sample(in double x, in double y, in ColorChannel color, out byte pixelValue)
        {
            var pixel = new Rgba32();
            Sample(in x, in y, ref pixel);
            pixel.GetChannelValue(in color, out pixelValue);
        }

        public virtual void SampleScaled(in double x, in double y, in ColorChannel color, out float pixelValue)
        {
            Sample(in x, in y, in color, out var _value);
            pixelValue = _value / 255f;
        }

        protected void GetTexCoord(in double x, in double y, out float fx, out float fy)
        {
            fx = (float)((Bounds.X + x * Bounds.Width) * Image.Width);
            fy = (float)((Bounds.Y + y * Bounds.Height) * Image.Height);
        }

        protected void WrapCoordX(ref int x)
        {
            if (Bounds.Width == 0) throw new ArgumentOutOfRangeException(nameof(Bounds));
            var width = (int)(Bounds.Width * Image.Width + 0.5f);
            while (x < Bounds.Left * (Image.Width - 1)) x += width;
            while (x > Bounds.Right * (Image.Width - 1)) x -= width;
        }

        protected void WrapCoordY(ref int y)
        {
            if (Bounds.Height == 0) throw new ArgumentOutOfRangeException(nameof(Bounds));
            var height = (int)(Bounds.Height * Image.Height + 0.5f);
            while (y < Bounds.Top * (Image.Height - 1)) y += height;
            while (y > Bounds.Bottom * (Image.Height - 1)) y -= height;
        }

        protected void ClampCoordX(ref int x)
        {
            if (Bounds.Width == 0) throw new ArgumentOutOfRangeException(nameof(Bounds));
            if (x < Bounds.Left * Image.Width) x = (int)(Bounds.Left * Image.Width);
            if (x >= Bounds.Right * Image.Width) x = (int)(Bounds.Right * Image.Width) - 1;
        }

        protected void ClampCoordY(ref int y)
        {
            if (Bounds.Height == 0) throw new ArgumentOutOfRangeException(nameof(Bounds));
            if (y < Bounds.Top * Image.Height) y = (int)(Bounds.Top * Image.Height);
            if (y >= Bounds.Bottom * Image.Height) y = (int)(Bounds.Bottom * Image.Height) - 1;
        }
    }
}
