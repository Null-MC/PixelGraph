using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;

namespace PixelGraph.Common.Samplers
{
    internal class AverageSampler<TPixel> : SamplerBase<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        public override void Sample(in float fx, in float fy, ref Rgba32 pixel)
        {
            var step = (int) Range;

            var pxMin = (int)fx;
            var pyMin = (int)fy;
            var pxMax = (int)(fx + step);
            var pyMax = (int)(fy + step);

            var color = new Vector4();

            for (var y = pyMin; y <= pyMax; y++) {
                var py = y;

                if (WrapY) WrapCoordY(ref py);
                else ClampCoordY(ref py);

                for (var x = pxMin; x <= pxMax; x++) {
                    var px = x;

                    if (WrapX) WrapCoordX(ref px);
                    else ClampCoordX(ref px);

                    color += Image[px, py].ToScaledVector4();
                }
            }

            if (step > 0) color /= MathF.Pow(step + 1, 2);

            pixel.FromScaledVector4(color);
        }
    }
}
