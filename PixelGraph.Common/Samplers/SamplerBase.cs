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
        public float Range {get; set;}
        public bool Wrap {get; set;}


        public abstract void Sample(in float fx, in float fy, ref Rgba32 pixel);

        public virtual void SampleScaled(in float fx, in float fy, ref Vector4 vector)
        {
            var pixel = new Rgba32();
            Sample(in fx, in fy, ref pixel);
            vector = pixel.ToScaledVector4();
        }

        public virtual void Sample(in float fx, in float fy, in ColorChannel color, ref byte pixelValue)
        {
            var pixel = new Rgba32();
            Sample(in fx, in fy, ref pixel);
            pixel.GetChannelValue(in color, out pixelValue);
        }

        public virtual void SampleScaled(in float fx, in float fy, in ColorChannel color, ref float pixelValue)
        {
            byte _value = 0;
            Sample(in fx, in fy, in color, ref _value);
            pixelValue = _value / 255f;
        }

        protected void WrapCoords(ref int x, ref int y)
        {
            while (x < 0) x += Image.Width;
            while (y < 0) y += Image.Height;

            while (x >= Image.Width) x -= Image.Width;
            while (y >= Image.Height) y -= Image.Height;
        }

        protected void ClampCoords(ref int x, ref int y)
        {
            if (x < 0) x = 0;
            if (y < 0) y = 0;

            if (x >= Image.Width) x = Image.Width - 1;
            if (y >= Image.Height) y = Image.Height - 1;
        }
    }
}
