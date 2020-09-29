using McPbrPipeline.Filters;
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


        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle) where TPixel : unmanaged, IPixel<TPixel>
        {
            return new NormalMapProcessor<TPixel>(this, source, configuration, sourceRectangle);
        }
    }

    internal class NormalMapProcessor<TPixel> : IImageProcessor<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly NormalMapProcessor processor;
        //private readonly Configuration configuration;
        private readonly Rectangle sourceRectangle;
        private readonly Image<TPixel> source;


        public NormalMapProcessor(NormalMapProcessor processor, Image<TPixel> source, Configuration configuration, Rectangle sourceRectangle)
        {
            this.processor = processor;
            //this.configuration = configuration;
            this.sourceRectangle = sourceRectangle;
            this.source = source;
        }

        public void Execute()
        {
            using var target = new Image<Rgba32>(sourceRectangle.Width, sourceRectangle.Height);

            var k = new float[3, 3];
            Vector3 angle;
            Vector2 derivative;

            for (var y = sourceRectangle.Top; y < sourceRectangle.Bottom; y++) {
                for (var x = sourceRectangle.Left; x < sourceRectangle.Right; x++) {
                    PopulateKernel_3x3(ref k, x, y);
                    GetSobelDerivative(ref k, out derivative);

                    angle.X = derivative.X;
                    angle.Y = derivative.Y;
                    angle.Z = 1f / (processor.Options.Strength * 100f);

                    // TODO: implement by-ref normalize
                    var normal = Vector3.Normalize(angle);

                    target[x, y] = new Rgba32(normal.X * 0.5f + 0.5f, normal.Y * 0.5f + 0.5f, normal.Z);
                }
            }

            var brush = new ImageBrush(target);
            source.Mutate(c => c.Fill(brush));
        }

        public void Dispose() {}

        private void PopulateKernel_3x3(ref float[,] kernel, int x, int y)
        {
            for (var kY = 0; kY < 3; kY++) {
                for (var kX = 0; kX < 3; kX++) {
                    if (kX == 1 && kY == 1) {
                        kernel[1, 1] = 0f;
                        continue;
                    }

                    kernel[kX, kY] = GetPixelFloat(x + kX - 1, y + kY - 1);
                }
            }
        }

        private float GetPixelFloat(int x, int y)
        {
            if (processor.Options.Wrap) {
                if (x < sourceRectangle.Left)
                    x += sourceRectangle.Width;

                if (x >= sourceRectangle.Right)
                    x -= sourceRectangle.Width;

                if (y < sourceRectangle.Top)
                    y += sourceRectangle.Height;

                if (y >= sourceRectangle.Bottom)
                    y -= sourceRectangle.Height;
            }
            else {
                if (x < sourceRectangle.Left)
                    x = sourceRectangle.Left;

                if (x >= sourceRectangle.Right)
                    x = sourceRectangle.Right - 1;

                if (y < sourceRectangle.Top)
                    y = sourceRectangle.Top;

                if (y >= sourceRectangle.Bottom)
                    y = sourceRectangle.Bottom - 1;
            }

            var pixel = source[x, y].ToVector4();

            return (pixel.X / 255.0f + pixel.Y / 255.0f + pixel.Z / 255.0f) / 3f;
        }

        private static void GetSobelDerivative(ref float[,] kernel, out Vector2 derivative)
        {
            derivative.X = kernel[0, 0] + kernel[0, 1] * 2f + kernel[0, 2] - kernel[2, 0] - kernel[2, 1] * 2f - kernel[2, 2];
            derivative.Y = kernel[0, 0] + kernel[1, 0] * 2f + kernel[2, 0] - kernel[0, 2] - kernel[1, 2] * 2f - kernel[2, 2];
        }

        //private static void GetPrewittDerivative(ref float[,] kernel, out Vector2 derivative)
        //{
        //    derivative.X = kernel[0, 0] * 3.0f + kernel[0, 1] * 10.0f + kernel[0, 2] * 3.0f - kernel[2, 0] * 3.0f - kernel[2, 1] * 10.0f - kernel[2, 2] * 3.0f;
        //    derivative.Y = kernel[0, 0] * 3.0f + kernel[1, 0] * 10.0f + kernel[2, 0] * 3.0f - kernel[0, 2] * 3.0f - kernel[1, 2] * 10.0f - kernel[2, 2] * 3.0f;
        //}
    }
}
