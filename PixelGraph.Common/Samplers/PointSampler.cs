using SixLabors.ImageSharp.PixelFormats;

namespace PixelGraph.Common.Samplers
{
    internal class PointSampler : SamplerBase
    {
        public override void Sample(in float fx, in float fy, out Rgba32 pixel)
        {
            var px = (int)fx;
            var py = (int)fy;
            pixel = Image[px, py];
        }
    }
}
