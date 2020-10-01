using McPbrPipeline.Internal;
using McPbrPipeline.Internal.Filtering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
using System.Numerics;

namespace McPbrPipeline.ImageProcessors
{
    internal class NormalMapProcessor : IImageProcessor
    {
        public NormalMapOptions Options {get; set;}


        public NormalMapProcessor(NormalMapOptions options = null)
        {
            Options = options ?? new NormalMapOptions();
        }

        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle) where TPixel : unmanaged, IPixel<TPixel>
        {
            return new NormalMapProcessor<TPixel>(this, source, sourceRectangle);
        }
    }

    internal class NormalMapProcessor<TPixel> : IImageProcessor<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly NormalMapProcessor processor;
        private readonly Rectangle sourceRectangle;
        private readonly Image<TPixel> source;


        public NormalMapProcessor(NormalMapProcessor processor, Image<TPixel> source, Rectangle sourceRectangle)
        {
            this.processor = processor;
            this.sourceRectangle = sourceRectangle;
            this.source = source;
        }

        public void Execute()
        {
            using var target = new Image<Rgba32>(sourceRectangle.Width, sourceRectangle.Height);
            var hasDepthScale = processor.Options.DepthScale > float.Epsilon;

            var k = new float[3, 3];
            var pixel = new Rgba32(0, 0, 0, 255);
            Vector2 derivative;
            Vector3 normal;

            for (var y = sourceRectangle.Top; y < sourceRectangle.Bottom; y++) {
                for (var x = sourceRectangle.Left; x < sourceRectangle.Right; x++) {
                    PopulateKernel_3x3(ref k, x, y);
                    GetSobelDerivative(ref k, out derivative);

                    normal.X = derivative.X;
                    normal.Y = derivative.Y;
                    normal.Z = 1f / processor.Options.Strength;

                    MathEx.Normalize(ref normal);

                    pixel.R = MathEx.Saturate(normal.X * 0.5f + 0.5f);
                    pixel.G = MathEx.Saturate(normal.Y * 0.5f + 0.5f);
                    pixel.B = MathEx.Saturate(normal.Z);

                    if (hasDepthScale) pixel.A = MathEx.Saturate(1f - (1f - k[1, 1]) * processor.Options.DepthScale);

                    target[x, y] = pixel;
                }
            }

            var brush = new ImageBrush(target);
            source.Mutate(c => c.Clear(brush));
        }

        public void Dispose() {}

        private void PopulateKernel_3x3(ref float[,] kernel, int x, int y)
        {
            for (var kY = 0; kY < 3; kY++) {
                for (var kX = 0; kX < 3; kX++) {
                    //if (kX == 1 && kY == 1) continue;

                    var pX = x + kX - 1;
                    var pY = y + kY - 1;

                    if (processor.Options.Wrap) Wrap(ref pX, ref pY);
                    else Clamp(ref pX, ref pY);

                    var pixel = source[pX, pY].ToScaledVector4();
                    kernel[kX, kY] = (pixel.X + pixel.Y + pixel.Z) / 3f;
                }
            }
        }

        private static void GetSobelDerivative(ref float[,] kernel, out Vector2 derivative)
        {
            var topSide = kernel[0,0] + 2f * kernel[0,1] + kernel[0,2];
            var bottomSide = kernel[2,0] + 2f * kernel[2,1] + kernel[2,2];
            var rightSide = kernel[0,2] + 2f * kernel[1,2] + kernel[2,2];
            var leftSide = kernel[0,0] + 2f * kernel[1,0] + kernel[2,0];

            derivative.Y = leftSide - rightSide;
            derivative.X = topSide - bottomSide;
        }

        private void Wrap(ref int x, ref int y)
        {
            if (x < sourceRectangle.Left)
                x += sourceRectangle.Width;

            if (x >= sourceRectangle.Right)
                x -= sourceRectangle.Width;

            if (y < sourceRectangle.Top)
                y += sourceRectangle.Height;

            if (y >= sourceRectangle.Bottom)
                y -= sourceRectangle.Height;
        }

        private void Clamp(ref int x, ref int y)
        {
            if (x < sourceRectangle.Left)
                x = sourceRectangle.Left;

            if (x >= sourceRectangle.Right)
                x = sourceRectangle.Right - 1;

            if (y < sourceRectangle.Top)
                y = sourceRectangle.Top;

            if (y >= sourceRectangle.Bottom)
                y = sourceRectangle.Bottom - 1;
        }
    }
}
