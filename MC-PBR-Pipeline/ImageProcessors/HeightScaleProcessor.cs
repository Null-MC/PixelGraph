using McPbrPipeline.Internal.Extensions;
using McPbrPipeline.Internal.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;

namespace McPbrPipeline.ImageProcessors
{
    internal class HeightScaleProcessor : IImageProcessor
    {
        private readonly Options options;


        public HeightScaleProcessor(Options options)
        {
            this.options = options;
        }

        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle) where TPixel : unmanaged, IPixel<TPixel>
        {
            return new Processor<TPixel>(configuration, source, sourceRectangle, options);
        }

        public class Options
        {
            public float Scale {get; set;} = 1f;
            public ColorChannel HeightChannel;
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
            private readonly ImageFrame<TPixel> sourceFrame;
            private readonly Options options;


            public RowOperation(ImageFrame<TPixel> sourceFrame, Options options)
            {
                this.sourceFrame = sourceFrame;
                this.options = options;
            }

            public void Invoke(int y)
            {
                var row = sourceFrame.GetPixelRowSpan(y);

                var pixel = new Rgba32();
                for (var x = 0; x < sourceFrame.Width; x++) {
                    row[x].ToRgba32(ref pixel);

                    if (options.HeightChannel.HasFlag(ColorChannel.Red))
                        pixel.R = MathEx.Saturate(1 - (1 - pixel.R / 255d) * options.Scale);

                    if (options.HeightChannel.HasFlag(ColorChannel.Green))
                        pixel.G = MathEx.Saturate(1 - (1 - pixel.G / 255d) * options.Scale);

                    if (options.HeightChannel.HasFlag(ColorChannel.Blue))
                        pixel.B = MathEx.Saturate(1 - (1 - pixel.B / 255d) * options.Scale);

                    if (options.HeightChannel.HasFlag(ColorChannel.Alpha))
                        pixel.A = MathEx.Saturate(1 - (1 - pixel.A / 255d) * options.Scale);

                    row[x].FromRgba32(pixel);
                }
            }
        }
    }
}
