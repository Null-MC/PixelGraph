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
        //protected const float HalfPixel = 0.5f + float.Epsilon;
        private const float RgbToLinear = 1f / 255f;

        public Image<TPixel> Image {get; set;}
        public float RangeX {get; set;}
        public float RangeY {get; set;}
        public bool WrapX {get; set;}
        public bool WrapY {get; set;}
        public Rectangle Bounds {get; set;}


        protected SamplerBase()
        {
            Bounds = Rectangle.Empty;
        }

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
            pixelValue = _value * RgbToLinear;
        }

        protected void GetTexCoord(in double x, in double y, out float fx, out float fy)
        {
            //Bounds.GetTexCoord(in x, in y, out var dx, out var dy);

            //fx = (float)(Bounds.Left + x * Bounds.Width) * Image.Width;
            //fy = (float)(Bounds.Top + y * Bounds.Height) * Image.Height;
            fx = (float)(Bounds.Left + x * Bounds.Width);
            fy = (float)(Bounds.Top + y * Bounds.Height);
        }

        protected void WrapCoordX(ref int x)
        {
            //var innerImageWidth = Image.Width - 1;
            //var outerBoundsWidth = (int)(Bounds.Width * Image.Width);
            //var left = (int)(Bounds.Left * innerImageWidth);
            //var right = (int)(Bounds.Right * innerImageWidth);

            //while (x < left) x += outerBoundsWidth;
            //while (x > right) x -= outerBoundsWidth;

            while (x < Bounds.Left) x += Bounds.Width;
            while (x >= Bounds.Right) x -= Bounds.Width;
        }

        protected void WrapCoordY(ref int y)
        {
            //var innerImageHeight = Image.Height - 1;
            //var outerBoundsHeight = (int)(Bounds.Height * Image.Height);
            //var top = (int)(Bounds.Top * innerImageHeight);
            //var bottom = (int)(Bounds.Bottom * innerImageHeight);

            //while (y < top) y += outerBoundsHeight;
            //while (y > bottom) y -= outerBoundsHeight;

            while (y < Bounds.Top) y += Bounds.Height;
            while (y >= Bounds.Bottom) y -= Bounds.Height;
        }

        protected void ClampCoordX(ref int x)
        {
            if (x < Bounds.Left) x = Bounds.Left;
            if (x >= Bounds.Right) x = Bounds.Right-1;
        }

        protected void ClampCoordY(ref int y)
        {
            if (y < Bounds.Top) y = Bounds.Top;
            if (y >= Bounds.Bottom) y = Bounds.Bottom-1;
        }
    }
}
