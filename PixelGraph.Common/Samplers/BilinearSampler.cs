using PixelGraph.Common.Extensions;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;

namespace PixelGraph.Common.Samplers
{
    internal class BilinearSampler<TPixel> : SamplerBase<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        public override void Sample(in float fx, in float fy, ref Rgba32 pixel)
        {
            var vector = new Vector4();
            SampleScaled(in fx, in fy, ref vector);
            pixel.FromScaledVector4(vector);
        }

        public override void SampleScaled(in float fx, in float fy, ref Vector4 vector)
        {
            var pxMin = (int)fx;
            var pxMax = pxMin + 1;
            var pyMin = (int)fy;
            var pyMax = pyMin + 1;

            var px = fx - pxMin;
            var py = fy - pyMin;

            if (Wrap) {
                WrapCoords(ref pxMin, ref pyMin);
                WrapCoords(ref pxMax, ref pyMax);
            }
            else {
                ClampCoords(ref pxMin, ref pyMin);
                ClampCoords(ref pxMax, ref pyMax);
            }

            var rowMin = Image.GetPixelRowSpan(pyMin);
            var rowMax = Image.GetPixelRowSpan(pyMax);

            var pixelMatrix = new Vector4[4];
            pixelMatrix[0] = rowMin[pxMin].ToScaledVector4();
            pixelMatrix[1] = rowMin[pxMax].ToScaledVector4();
            pixelMatrix[2] = rowMax[pxMin].ToScaledVector4();
            pixelMatrix[3] = rowMax[pxMax].ToScaledVector4();

            MathEx.Lerp(in pixelMatrix[0], in pixelMatrix[1], in px, out var zMin);
            MathEx.Lerp(in pixelMatrix[2], in pixelMatrix[3], in px, out var zMax);
            MathEx.Lerp(in zMin, in zMax, in py, out vector);
        }

        public override void Sample(in float fx, in float fy, in ColorChannel color, ref byte pixelValue)
        {
            var pixel = new Rgba32();
            Sample(in fx, in fy, ref pixel);
            pixel.GetChannelValue(color, out pixelValue);
        }

        public override void SampleScaled(in float fx, in float fy, in ColorChannel color, ref float pixelValue)
        {
            var vector = new Vector4();
            SampleScaled(in fx, in fy, ref vector);
            vector.GetChannelValue(color, out pixelValue);
        }
    }
}
