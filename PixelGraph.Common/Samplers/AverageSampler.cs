using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;

namespace PixelGraph.Common.Samplers
{
    internal class AverageSampler<TPixel> : SamplerBase<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        public override void Sample(in float fx, in float fy, ref Rgba32 pixel)
        {
            var pxMin = (int)fx;
            var pxMax = (int)(fx + Range - 1);
            var pyMin = (int)fy;
            var pyMax = (int)(fy + Range - 1);

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

            var count = (pxMax - pxMin + 1) * (pyMax - pyMin + 1);
            color /= count;

            pixel.FromScaledVector4(color);
        }
    }
}
