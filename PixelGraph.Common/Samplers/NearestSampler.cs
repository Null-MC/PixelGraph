using SixLabors.ImageSharp.PixelFormats;

namespace PixelGraph.Common.Samplers
{
    internal class NearestSampler<TPixel> : SamplerBase<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        public override void Sample(in float fx, in float fy, ref Rgba32 pixel)
        {
            var px = (int) fx;
            var py = (int) fy;

            if (WrapX) WrapCoordX(ref px);
            else ClampCoordX(ref px);

            if (WrapY) WrapCoordY(ref py);
            else ClampCoordY(ref py);

            Image[px, py].ToRgba32(ref pixel);
        }
    }
}
