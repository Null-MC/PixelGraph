using McPbrPipeline.Internal.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;

namespace McPbrPipeline.ImageProcessors
{
    internal class OverlayProcessor : IImageProcessor
    {
        private readonly Options options;


        public OverlayProcessor(in Options options)
        {
            this.options = options;
        }

        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle) where TPixel : unmanaged, IPixel<TPixel>
        {
            return new Processor<TPixel>(configuration, source, sourceRectangle, options);
        }

        public class Options
        {
            public Image<Rgba32> Source {get; set;}
            public ColorChannel RedSource {get; set;}
            public ColorChannel GreenSource {get; set;}
            public ColorChannel BlueSource {get; set;}
            public ColorChannel AlphaSource {get; set;}


            public void Set(ColorChannel source, ColorChannel destination)
            {
                switch (destination) {
                    case ColorChannel.Red:
                        RedSource = source;
                        break;
                    case ColorChannel.Green:
                        GreenSource = source;
                        break;
                    case ColorChannel.Blue:
                        BlueSource = source;
                        break;
                    case ColorChannel.Alpha:
                        AlphaSource = source;
                        break;
                }
            }
        }

        private class Processor<TPixel> : ImageProcessor<TPixel> where TPixel : unmanaged, IPixel<TPixel>
        {
            private readonly Options options;


            public Processor(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle, Options options)
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
            //private readonly ImageFrame<Rgba32> sourceFrame;
            private readonly ImageFrame<TPixel> targetFrame;
            private readonly Options options;


            public RowOperation(
                ImageFrame<TPixel> targetFrame,
                Options options)
            {
                this.targetFrame = targetFrame;
                this.options = options;

                //sourceFrame = options.Source.Frames.RootFrame;
            }

            public void Invoke(int y)
            {
                var sourceRow = options.Source.GetPixelRowSpan(y);
                var targetRow = targetFrame.GetPixelRowSpan(y);

                var pixelSource = new Rgba32();
                var pixelTarget = new Rgba32();

                for (var x = 0; x < targetFrame.Width; x++) {
                    sourceRow[x].ToRgba32(ref pixelSource);
                    targetRow[x].ToRgba32(ref pixelTarget);

                    if (options.RedSource != ColorChannel.None)
                        pixelTarget.R = GetValue(options.RedSource, in pixelSource);

                    if (options.GreenSource != ColorChannel.None)
                        pixelTarget.G = GetValue(options.GreenSource, in pixelSource);

                    if (options.BlueSource != ColorChannel.None)
                        pixelTarget.B = GetValue(options.BlueSource, in pixelSource);

                    if (options.AlphaSource != ColorChannel.None)
                        pixelTarget.A = GetValue(options.AlphaSource, in pixelSource);

                    targetRow[x].FromRgba32(pixelTarget);
                }
            }

            private static byte GetValue(in ColorChannel color, in Rgba32 sourcePixel)
            {
                return color switch {
                    ColorChannel.Red => sourcePixel.R,
                    ColorChannel.Green => sourcePixel.G,
                    ColorChannel.Blue => sourcePixel.B,
                    ColorChannel.Alpha => sourcePixel.A,
                    _ => 0,
                };
            }
        }
    }
}
