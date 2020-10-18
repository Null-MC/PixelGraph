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
            public ColorChannel RedSource;
            public PixelAction RedAction;
            public ColorChannel GreenSource;
            public PixelAction GreenAction;
            public ColorChannel BlueSource;
            public PixelAction BlueAction;
            public ColorChannel AlphaSource;
            public PixelAction AlphaAction;


            public void Set(ColorChannel source, ColorChannel destination, PixelAction action = null)
            {
                switch (destination) {
                    case ColorChannel.Red:
                        RedSource = source;
                        RedAction = action;
                        break;
                    case ColorChannel.Green:
                        GreenSource = source;
                        GreenAction = action;
                        break;
                    case ColorChannel.Blue:
                        BlueSource = source;
                        BlueAction = action;
                        break;
                    case ColorChannel.Alpha:
                        AlphaSource = source;
                        AlphaAction = action;
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
            private readonly ImageFrame<TPixel> targetFrame;
            private readonly Options options;


            public RowOperation(
                ImageFrame<TPixel> targetFrame,
                Options options)
            {
                this.targetFrame = targetFrame;
                this.options = options;
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
                        GetValue(in pixelSource, in options.RedSource, options.RedAction, out pixelTarget.R);

                    if (options.GreenSource != ColorChannel.None)
                        GetValue(in pixelSource, in options.GreenSource, options.GreenAction, out pixelTarget.G);

                    if (options.BlueSource != ColorChannel.None)
                        GetValue(in pixelSource, in options.BlueSource, options.BlueAction, out pixelTarget.B);

                    if (options.AlphaSource != ColorChannel.None)
                        GetValue(in pixelSource, in options.AlphaSource, options.AlphaAction, out pixelTarget.A);

                    targetRow[x].FromRgba32(pixelTarget);
                }
            }

            private static void GetValue(in Rgba32 sourcePixel, in ColorChannel color, PixelAction action, out byte value)
            {
                value = GetSourceValue(in sourcePixel, in color);
                action?.Invoke(ref value);
            }

            private static byte GetSourceValue(in Rgba32 sourcePixel, in ColorChannel color)
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
