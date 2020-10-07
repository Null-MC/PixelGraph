using McPbrPipeline.Internal;
using McPbrPipeline.Internal.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using System;
using System.Numerics;
using McPbrPipeline.Internal.Extensions;

namespace McPbrPipeline.ImageProcessors
{
    internal class NormalRestoreProcessor : IImageProcessor
    {
        private readonly Options options;


        public NormalRestoreProcessor(Options options)
        {
            this.options = options;
        }

        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle) where TPixel : unmanaged, IPixel<TPixel>
        {
            return new Processor<TPixel>(source, configuration, sourceRectangle, options);
        }

        public class Options
        {
            public ColorChannel NormalX = ColorChannel.None;
            public ColorChannel NormalY = ColorChannel.None;
            public ColorChannel NormalZ = ColorChannel.None;


            public bool HasAllMappings()
            {
                if (NormalX == ColorChannel.None) return false;
                if (NormalY == ColorChannel.None) return false;
                if (NormalZ == ColorChannel.None) return false;
                return true;
            }
        }

        private class Processor<TPixel> : ImageProcessor<TPixel> where TPixel : unmanaged, IPixel<TPixel>
        {
            private readonly Options options;


            public Processor(Image<TPixel> source, Configuration configuration, Rectangle sourceRectangle, Options options)
                : base(configuration, source, sourceRectangle)
            {
                this.options = options;
            }

            protected override void OnFrameApply(ImageFrame<TPixel> source)
            {
                var operation = new RowOperation<TPixel>(source, options);
                ParallelRowIterator.IterateRows(Configuration, SourceRectangle, in operation);
            }
        }

        private readonly struct RowOperation<TPixel> : IRowOperation where TPixel : unmanaged, IPixel<TPixel>
        {
            private readonly ImageFrame<TPixel> sourceFrame;
            private readonly Options options;


            public RowOperation(
                ImageFrame<TPixel> sourceFrame,
                in Options options)
            {
                this.sourceFrame = sourceFrame;
                this.options = options;
            }

            public void Invoke(int y)
            {
                var row = sourceFrame.GetPixelRowSpan(y);

                var vector = new Vector3();
                var pixel = new Rgba32();
                for (var x = 0; x < sourceFrame.Width; x++) {
                    row[x].ToRgba32(ref pixel);

                    vector.X = Get(in pixel, options.NormalX) / 255f;
                    vector.Y = Get(in pixel, options.NormalY) / 255f;

                    var dot = vector.X * vector.X + vector.Y * vector.Y;
                    vector.Z = (float)Math.Sqrt(1f - dot);

                    // TODO: not "necessary"
                    //MathEx.Normalize(ref vector);

                    switch (options.NormalZ) {
                        case ColorChannel.Red:
                            pixel.R = MathEx.Saturate(vector.Z);
                            break;
                        case ColorChannel.Green:
                            pixel.G = MathEx.Saturate(vector.Z);
                            break;
                        case ColorChannel.Blue:
                            pixel.B = MathEx.Saturate(vector.Z);
                            break;
                        case ColorChannel.Alpha:
                            pixel.A = MathEx.Saturate(vector.Z);
                            break;
                    }

                    row[x].FromRgba32(pixel);
                }
            }

            private static byte Get(in Rgba32 pixel, ColorChannel channel)
            {
                return channel switch {
                    ColorChannel.Red => pixel.R,
                    ColorChannel.Green => pixel.G,
                    ColorChannel.Blue => pixel.B,
                    ColorChannel.Alpha => pixel.A,
                    _ => 0,
                };
            }
        }
    }
}
