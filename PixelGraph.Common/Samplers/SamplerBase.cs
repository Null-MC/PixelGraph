using PixelGraph.Common.Extensions;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;

namespace PixelGraph.Common.Samplers
{
    internal abstract class SamplerBase : ISampler
    {
        public Image<Rgba32> Image {get; set;}
        public bool Wrap {get; set;}


        public abstract void Sample(in float fx, in float fy, out Rgba32 pixel);

        public virtual void SampleScaled(in float fx, in float fy, out Vector4 vector)
        {
            Sample(in fx, in fy, out var pixel);
            vector = pixel.ToScaledVector4();
        }

        public virtual void Sample(in float fx, in float fy, in ColorChannel color, out byte pixelValue)
        {
            Sample(in fx, in fy, out var pixel);
            pixel.GetChannelValue(in color, out pixelValue);
        }

        public virtual void SampleScaled(in float fx, in float fy, in ColorChannel color, out float pixelValue)
        {
            Sample(in fx, in fy, in color, out var _value);
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
