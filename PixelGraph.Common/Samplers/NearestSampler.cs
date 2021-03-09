using SixLabors.ImageSharp.PixelFormats;
using System;

namespace PixelGraph.Common.Samplers
{
    internal class NearestSampler<TPixel> : SamplerBase<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        public override void Sample(in float x, in float y, ref Rgba32 pixel)
        {
            GetTexCoord(in x, in y, out var fx, out var fy);

            var px = (int)MathF.Floor(fx + HalfPixel);
            var py = (int)MathF.Floor(fy + HalfPixel);

            if (Frame.HasValue) {
                var bounds = GetFrameBounds();

                if (WrapX) WrapCoordX(ref px, ref bounds);
                else ClampCoordX(ref px, ref bounds);

                if (WrapY) WrapCoordY(ref py, ref bounds);
                else ClampCoordY(ref py, ref bounds);
            }
            else {
                if (WrapX) WrapCoordX(ref px);
                else ClampCoordX(ref px);

                if (WrapY) WrapCoordY(ref py);
                else ClampCoordY(ref py);
            }

            Image[px, py].ToRgba32(ref pixel);
        }
    }
}
