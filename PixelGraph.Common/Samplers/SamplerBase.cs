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
        private const float RgbToLinear = 1f / 255f;

        protected Rectangle Bounds;

        public Image<TPixel> Image {get; set;}
        public float RangeX {get; set;}
        public float RangeY {get; set;}
        public bool WrapX {get; set;}
        public bool WrapY {get; set;}


        protected SamplerBase()
        {
            Bounds = Rectangle.Empty;
        }

        public void SetBounds(in UVRegion region)
        {
            if (Image == null) throw new ApplicationException("Unable to set bounds when image is undefined!");

            Bounds = region.ScaleTo(Image.Width, Image.Height);
        }

        public abstract IRowSampler<TPixel> ForRow(in double y);

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
            fx = (float)(Bounds.Left + x * Bounds.Width);
            fy = (float)(Bounds.Top + y * Bounds.Height);
        }

        protected void GetTexCoordX(in double x, out float fx)
        {
            fx = (float)(Bounds.Left + x * Bounds.Width);
        }

        protected void GetTexCoordY(in double y, out float fy)
        {
            fy = (float)(Bounds.Top + y * Bounds.Height);
        }

        protected void NormalizeTexCoordX(ref int px)
        {
            if (WrapX) TexCoordHelper.WrapCoordX(ref px, in Bounds);
            else TexCoordHelper.ClampCoordX(ref px, in Bounds);
        }

        protected void NormalizeTexCoordY(ref int py)
        {
            if (WrapY) TexCoordHelper.WrapCoordY(ref py, in Bounds);
            else TexCoordHelper.ClampCoordY(ref py, in Bounds);
        }

        protected void NormalizeTexCoord(ref int px, ref int py)
        {
            NormalizeTexCoordX(ref px);
            NormalizeTexCoordY(ref py);
        }
    }
}
