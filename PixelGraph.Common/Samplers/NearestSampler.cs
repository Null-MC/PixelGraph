using SixLabors.ImageSharp.PixelFormats;

namespace PixelGraph.Common.Samplers
{
    internal class NearestSampler : SamplerBase
    {
        public override void Sample(in float fx, in float fy, out Rgba32 pixel)
        {
            var px = (int) (fx + 0.5f);
            var py = (int) (fy + 0.5f);

            if (Wrap) WrapCoords(ref px, ref py);
            else ClampCoords(ref px, ref py);

            pixel = Image[px, py];
        }
    }
}
