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
        public Image<TPixel> Image {get; set;}
        public float RangeX {get; set;}
        public float RangeY {get; set;}
        public bool WrapX {get; set;}
        public bool WrapY {get; set;}


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

        protected void WrapCoordX(ref int x)
        {
            while (x < 0) x += Image.Width;
            while (x >= Image.Width) x -= Image.Width;
        }

        protected void WrapCoordY(ref int y)
        {
            while (y < 0) y += Image.Height;
            while (y >= Image.Height) y -= Image.Height;
        }

        protected void ClampCoordX(ref int x)
        {
            if (x < 0) x = 0;
            if (x >= Image.Width) x = Image.Width - 1;
        }

        protected void ClampCoordY(ref int y)
        {
            if (y < 0) y = 0;
            if (y >= Image.Height) y = Image.Height - 1;
        }
    }
}
