using McPbrPipeline.Internal;
using McPbrPipeline.Internal.Filtering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;

namespace McPbrPipeline.ImageProcessors
{
    internal class ScaleProcessor : IImageProcessor
    {
        private readonly ScaleOptions options;


        public ScaleProcessor(ScaleOptions options)
        {
            this.options = options;
        }

        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle) where TPixel : unmanaged, IPixel<TPixel>
        {
            return new ScaleProcessor<TPixel>(source, configuration, sourceRectangle, options);
        }
    }

    internal class ScaleProcessor<TPixel> : ImageProcessor<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly ScaleOptions options;


        public ScaleProcessor(Image<TPixel> source, Configuration configuration, Rectangle sourceRectangle, ScaleOptions options)
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
            private readonly ImageFrame<TPixel> sourceFrame;
            private readonly ScaleOptions options;


            public RowOperation(
                ImageFrame<TPixel> sourceFrame,
                in ScaleOptions options)
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

                    pixel.R = Filter(in pixel.R, in options.Red);
                    pixel.G = Filter(in pixel.G, in options.Green);
                    pixel.B = Filter(in pixel.B, in options.Blue);
                    pixel.A = Filter(in pixel.A, in options.Alpha);

                    row[x].FromRgba32(pixel);
                }
            }

            private static byte Filter(in byte value, in float scale)
            {
                return MathEx.Saturate(value / 255f * scale);
            }
        }
    }
}
