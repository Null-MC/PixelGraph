using SixLabors.ImageSharp.PixelFormats;
using System;

namespace PixelGraph.Common.Samplers
{
    internal class NearestSampler<TPixel> : SamplerBase<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        public override void Sample(in float x, in float y, ref Rgba32 pixel)
        {
            var px = (int)MathF.Floor(x * Image.Width);
            var py = (int)MathF.Floor(y * Image.Height);

            if (WrapX) WrapCoordX(ref px);
            else ClampCoordX(ref px);

            if (WrapY) WrapCoordY(ref py);
            else ClampCoordY(ref py);

            Image[px, py].ToRgba32(ref pixel);
        }
    }
}
