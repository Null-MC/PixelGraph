using McPbrPipeline.Internal.Filtering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

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
            Vector2 derivative;
            Vector3 normal;

            for (var y = sourceRectangle.Top; y < sourceRectangle.Bottom; y++) {
                for (var x = sourceRectangle.Left; x < sourceRectangle.Right; x++) {
                    PopulateKernel_3x3(ref k, x, y);
                    GetSobelDerivative(ref k, out derivative);

                    normal.X = derivative.X;
                    normal.Y = derivative.Y;
                    normal.Z = 1f / (processor.Options.Strength * 100f);

                    Normalize(ref normal);

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
                    if (kX == 1 && kY == 1) continue;
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
            var topSide = kernel[0,0] + 2f * kernel[0,1] + kernel[0,2];
            var bottomSide = kernel[2,0] + 2f * kernel[2,1] + kernel[2,2];
            var rightSide = kernel[0,2] + 2f * kernel[1,2] + kernel[2,2];
            var leftSide = kernel[0,0] + 2f * kernel[1,0] + kernel[2,0];

            derivative.Y = rightSide - leftSide;
            derivative.X = bottomSide - topSide;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Normalize(ref Vector3 value)
        {
            float length;
            if (Vector.IsHardwareAccelerated) {
                length = value.Length();
            }
            else {
                var ls = value.X * value.X + value.Y * value.Y + value.Z * value.Z;
                length = MathF.Sqrt(ls);
            }

            value.X /= length;
            value.Y /= length;
            value.Z /= length;
        }
    }
}
