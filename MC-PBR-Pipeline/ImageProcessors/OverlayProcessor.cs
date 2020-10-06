using McPbrPipeline.Internal;
using McPbrPipeline.Internal.Filtering;
using McPbrPipeline.Internal.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using System;

namespace McPbrPipeline.ImageProcessors
{
    internal class OverlayProcessor : IImageProcessor
    {
        private readonly OverlayOptions options;


        public OverlayProcessor(in OverlayOptions options)
        {
            this.options = options;
        }

        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle) where TPixel : unmanaged, IPixel<TPixel>
        {
            return new OverlayProcessor<TPixel>(configuration, source, sourceRectangle, options);
        }
    }

    internal class OverlayProcessor<TPixel> : ImageProcessor<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly OverlayOptions options;


        public OverlayProcessor(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle, OverlayOptions options)
            : base(configuration, source, sourceRectangle)
        {
            this.options = options;
        }

        protected override void OnFrameApply(ImageFrame<TPixel> source)
        {
            var operation = new RowOperation(source, options);
            ParallelRowIterator.IterateRows(Configuration, SourceRectangle, in operation);
        }

        private readonly struct RowOperation : IRowOperation
        {
            private readonly ImageFrame<Rgba32> sourceFrame;
            private readonly ImageFrame<TPixel> targetFrame;
            private readonly OverlayOptions options;


            public RowOperation(
                ImageFrame<TPixel> targetFrame,
                OverlayOptions options)
            {
                this.targetFrame = targetFrame;
                this.options = options;

                sourceFrame = options.Source.Frames.RootFrame;
            }

            public void Invoke(int y)
            {
                var sourceRow = sourceFrame.GetPixelRowSpan(y);
                var targetRow = targetFrame.GetPixelRowSpan(y);

                var pixelSource = new Rgba32();
                var pixelTarget = new Rgba32();

                for (var x = 0; x < targetFrame.Width; x++) {
                    sourceRow[x].ToRgba32(ref pixelSource);
                    targetRow[x].ToRgba32(ref pixelTarget);

                    if (options.RedSource != ColorChannel.None) {
                        pixelTarget.R = GetChannel(options.RedSource, in pixelSource, in options.RedPower);
                    }

                    if (options.GreenSource != ColorChannel.None)
                        pixelTarget.G = GetChannel(options.GreenSource, in pixelSource, in options.GreenPower);

                    if (options.BlueSource != ColorChannel.None)
                        pixelTarget.B = GetChannel(options.BlueSource, in pixelSource, in options.BluePower);

                    if (options.AlphaSource != ColorChannel.None)
                        pixelTarget.A = GetChannel(options.AlphaSource, in pixelSource, in options.AlphaPower);

                    targetRow[x].FromRgba32(pixelTarget);
                }
            }

            private static byte GetChannel(in ColorChannel channel, in Rgba32 sourcePixel, in short power)
            {
                var result = channel switch {
                    ColorChannel.Red => sourcePixel.R,
                    ColorChannel.Green => sourcePixel.G,
                    ColorChannel.Blue => sourcePixel.B,
                    ColorChannel.Alpha => sourcePixel.A,
                    _ => (byte)0,
                };

                if (power > 0) return MathEx.Saturate(Math.Pow(result / 255d, 2));
                if (power < 0) return (byte)Math.Sqrt(result / 255d);
                return result;
            }
        }
    }
}
