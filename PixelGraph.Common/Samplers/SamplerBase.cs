using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace PixelGraph.Common.Samplers
{
    internal abstract class SamplerBase<TPixel> : ISampler<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
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

        public abstract IRowSampler ForRow(in double y);

        public abstract void SampleScaled(in double x, in double y, in ColorChannel color, out float pixelValue);

        protected void GetTexCoord(in double x, in double y, out float fx, out float fy)
        {
            //fx = (float)(Bounds.Left + x * Bounds.Width);
            //fy = (float)(Bounds.Top + y * Bounds.Height);
            GetTexCoordX(in x, out fx);
            GetTexCoordY(in y, out fy);
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
