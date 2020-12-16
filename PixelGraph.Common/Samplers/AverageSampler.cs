using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;

namespace PixelGraph.Common.Samplers
{
    internal class AverageSampler : SamplerBase
    {
        public override void Sample(in float fx, in float fy, out Rgba32 pixel)
        {
            var pxMin = (int)(fx);
            var pxMax = (int)(fx + Range - 1);
            var pyMin = (int)(fy);
            var pyMax = (int)(fy + Range - 1);

            var color = new Vector4();

            for (var y = pyMin; y <= pyMax; y++) {
                for (var x = pxMin; x <= pxMax; x++) {
                    var px = x;
                    var py = y;

                    if (Wrap) WrapCoords(ref px, ref py);
                    else ClampCoords(ref px, ref py);

                    color += Image[px, py].ToScaledVector4();
                }
            }

            var count = (pxMax - pxMin + 1) * (pyMax - pyMin + 1);
            color /= count;

            pixel = new Rgba32();
            pixel.FromScaledVector4(color);
        }
    }
}
