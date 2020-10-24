using McPbrPipeline.Internal.Extensions;
using McPbrPipeline.Internal.PixelOperations;
using McPbrPipeline.Internal.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;

namespace McPbrPipeline.ImageProcessors
{
    internal class NormalMapProcessor : PixelComposeProcessor
    {
        private readonly Options options;
        private readonly Lazy<Random> random;


        public NormalMapProcessor(Options options)
        {
            this.options = options;

            random = new Lazy<Random>();
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

            if (options.Noise > float.Epsilon) {
                normal.X += ((float)random.Value.NextDouble()*2f - 1f) * options.Noise;
                normal.Y += ((float)random.Value.NextDouble()*2f - 1f) * options.Noise;
            }

            MathEx.Normalize(ref normal);

            MathEx.Saturate(normal.X * 0.5f + 0.5f, out pixel.R);
            MathEx.Saturate(normal.Y * 0.5f + 0.5f, out pixel.G);
            MathEx.Saturate(normal.Z, out pixel.B);
            pixel.A = 255;
        }

        private void PopulateKernel_3x3(ref float[,] kernel, in PixelContext context)
        {
            var pixel = new Rgba32();

            for (var kY = 0; kY < 3; kY++) {
                for (var kX = 0; kX < 3; kX++) {
                    var pX = context.X + kX - 1;
                    var pY = context.Y + kY - 1;

                    if (options.Wrap) context.Wrap(ref pX, ref pY);
                    else context.Clamp(ref pX, ref pY);

                    // TODO: Add read-row caching
                    options.Source[pX, pY].ToRgba32(ref pixel);
                    pixel.GetChannelValue(in options.HeightChannel, out var height);
                    kernel[kX, kY] = height / 255f;
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
            //public int DownSample {get; set;} = 1;
            public float Strength = 1f;
            public float Noise = 0f;
            //public float Blur {get; set;} = 0f;
            public bool Wrap = true;
        }
    }
}
