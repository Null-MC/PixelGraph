using SixLabors.ImageSharp.PixelFormats;

namespace PixelGraph.Common.Samplers
{
    internal class NearestSampler : SamplerBase
    {
        public override void Sample(in float fx, in float fy, out Rgba32 pixel)
        {
            var px = (int) fx;
            var py = (int) fy;

            if (Wrap) WrapCoords(ref px, ref py);
            else ClampCoords(ref px, ref py);

            pixel = Image[px, py];
        }
    }
}
