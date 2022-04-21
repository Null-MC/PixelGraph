using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;

namespace PixelGraph.Common.Samplers
{
    internal abstract class RowSamplerBase<TPixel> : IRowSampler<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly int offsetX;
        private readonly int offsetY;

        //public ReadOnlySpan<TPixel> Row;
        public float RangeX {get; set;}
        public float RangeY {get; set;}
        public bool WrapX {get; set;}
        public bool WrapY {get; set;}
        public Rectangle Bounds {get; set;}


        protected RowSamplerBase(int offsetX, int offsetY)
        {
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            //Bounds = Rectangle.Empty;
        }
        
        public void Sample(in double x, in double y, ref Rgba32 pixel)
        {
            throw new NotImplementedException();
        }

        public void SampleScaled(in double x, in double y, out Vector4 pixel)
        {
            throw new NotImplementedException();
        }

        public void Sample(in double x, in double y, in ColorChannel color, out byte pixelValue)
        {
            throw new NotImplementedException();
        }

        public void SampleScaled(in double x, in double y, in ColorChannel color, out float pixelValue)
        {
            throw new NotImplementedException();
        }
    }
}
