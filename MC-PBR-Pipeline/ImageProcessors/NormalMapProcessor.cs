using System;
using McPbrPipeline.Internal.Extensions;
using McPbrPipeline.Internal.Textures;
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
        private readonly Options options;


        public NormalMapProcessor(Options options)
        {
            this.options = options;
        }

        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle) where TPixel : unmanaged, IPixel<TPixel>
        {
            return new Processor<TPixel>(source, sourceRectangle, options);
        }

        public class Options
        {
            public ColorChannel HeightChannel {get; set;}
            //public int DownSample {get; set;} = 1;
            public float Strength {get; set;} = 1f;
            public float Noise {get; set;} = 0f;
            //public float Blur {get; set;} = 0f;
            public bool Wrap {get; set;} = true;
        }

        private class Processor<TPixel> : IImageProcessor<TPixel> where TPixel : unmanaged, IPixel<TPixel>
        {
            private readonly Options options;
            private readonly Rectangle sourceRectangle;
            private readonly Image<TPixel> source;


            public Processor(Image<TPixel> source, Rectangle sourceRectangle, Options options)
            {
                this.sourceRectangle = sourceRectangle;
                this.source = source;
                this.options = options;
            }

            public void Execute()
            {
                using var target = new Image<Rgba32>(sourceRectangle.Width, sourceRectangle.Height);

                var k = new float[3, 3];
                var pixel = new Rgba32(0, 0, 0, 255);
                var random = new Lazy<Random>();
                var hasNoise = options.Noise > float.Epsilon;
                Vector2 derivative;
                Vector3 normal;

                for (var y = sourceRectangle.Top; y < sourceRectangle.Bottom; y++) {
                    for (var x = sourceRectangle.Left; x < sourceRectangle.Right; x++) {
                        PopulateKernel_3x3(ref k, x, y);
                        GetSobelDerivative(ref k, out derivative);

                        normal.X = derivative.X;
                        normal.Y = derivative.Y;
                        normal.Z = 1f / options.Strength;

                        if (hasNoise) {
                            normal.X += ((float)random.Value.NextDouble()*2f - 1f) * options.Noise;
                            normal.Y += ((float)random.Value.NextDouble()*2f - 1f) * options.Noise;
                        }

                        MathEx.Normalize(ref normal);

                        pixel.R = MathEx.Saturate(normal.X * 0.5f + 0.5f);
                        pixel.G = MathEx.Saturate(normal.Y * 0.5f + 0.5f);
                        pixel.B = MathEx.Saturate(normal.Z);

                        target[x, y] = pixel;
                    }
                }

                var brush = new ImageBrush(target);
                source.Mutate(c => c.Clear(brush));
            }

            public void Dispose() {}

            private void PopulateKernel_3x3(ref float[,] kernel, int x, int y)
            {
                var pixel = new Rgba32();
                for (var kY = 0; kY < 3; kY++) {
                    for (var kX = 0; kX < 3; kX++) {
                        var pX = x + kX - 1;
                        var pY = y + kY - 1;

                        if (options.Wrap) Wrap(ref pX, ref pY);
                        else Clamp(ref pX, ref pY);

                        source[pX, pY].ToRgba32(ref pixel);
                        kernel[kX, kY] = GetHeightValue(in pixel);
                    }
                }
            }

            private float GetHeightValue(in Rgba32 pixel)
            {
                return options.HeightChannel switch {
                    ColorChannel.Red => pixel.R / 255f,
                    ColorChannel.Green => pixel.G / 255f,
                    ColorChannel.Blue => pixel.B / 255f,
                    ColorChannel.Alpha => pixel.A / 255f,
                    _ => 0f,
                };
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
}
