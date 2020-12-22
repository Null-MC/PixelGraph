using PixelGraph.Common.Extensions;
using PixelGraph.Common.PixelOperations;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;

namespace PixelGraph.Common.ImageProcessors
{
    internal class NormalMapProcessor : PixelProcessor
    {
        private readonly Options options;


        public NormalMapProcessor(Options options)
        {
            this.options = options;
        }

        protected override void ProcessPixel(ref Rgba32 pixel, in PixelContext context)
        {
            var k = new float[3, 3];
            PopulateKernel_3x3(ref k, in context);
            GetSobelDerivative(ref k, out var derivative);

            Vector3 normal;
            normal.X = derivative.X;
            normal.Y = derivative.Y;
            normal.Z = 1f / options.Strength;
            MathEx.Normalize(ref normal);

            pixel.SetChannelValueScaled(ColorChannel.Red, normal.X * 0.5f + 0.5f);
            pixel.SetChannelValueScaled(ColorChannel.Green, normal.Y * 0.5f + 0.5f);
            pixel.SetChannelValueScaled(ColorChannel.Blue, normal.Z);
        }

        private void PopulateKernel_3x3(ref float[,] kernel, in PixelContext context)
        {
            for (var kY = 0; kY < 3; kY++) {
                var pY = context.Y + kY - 1;

                if (options.WrapY) context.WrapY(ref pY);
                else context.ClampY(ref pY);

                var row = options.Source.GetPixelRowSpan(pY);

                for (var kX = 0; kX < 3; kX++) {
                    var pX = context.X + kX - 1;

                    if (options.WrapX) context.WrapX(ref pX);
                    else context.ClampX(ref pX);

                    // TODO: Add read-row caching
                    row[pX].GetChannelValueScaled(in options.HeightChannel, out kernel[kX, kY]);
                }
            }
        }

        private static void GetSobelDerivative(ref float[,] kernel, out Vector2 derivative)
        {
            var topSide = kernel[0, 0] + 2f * kernel[0, 1] + kernel[0, 2];
            var bottomSide = kernel[2, 0] + 2f * kernel[2, 1] + kernel[2, 2];
            var rightSide = kernel[0, 2] + 2f * kernel[1, 2] + kernel[2, 2];
            var leftSide = kernel[0, 0] + 2f * kernel[1, 0] + kernel[2, 0];

            derivative.Y = leftSide - rightSide;
            derivative.X = topSide - bottomSide;
        }

        public class Options
        {
            public Image<Rgba32> Source;
            public ColorChannel HeightChannel;
            public float Strength = 1f;
            public bool WrapX = true;
            public bool WrapY = true;
        }
    }
}
